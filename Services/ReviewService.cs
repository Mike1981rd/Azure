using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebsiteBuilderCacheService _cacheService;

        public ReviewService(ApplicationDbContext context, IWebsiteBuilderCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<ReviewListResponseDto> GetReviewsAsync(int companyId, ReviewFilterDto filter)
        {
            var query = _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .Include(r => r.Media)
                .Include(r => r.RepliedBy)
                .Where(r => r.CompanyId == companyId);

            // Apply filters
            if (filter.ProductId.HasValue)
                query = query.Where(r => r.ProductId == filter.ProductId.Value);
                
            if (filter.RoomId.HasValue)
                query = query.Where(r => r.RoomId == filter.RoomId.Value);

            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);

            if (filter.Rating.HasValue)
                query = query.Where(r => r.Rating == filter.Rating.Value);

            if (filter.IsVerifiedPurchase.HasValue)
                query = query.Where(r => r.IsVerifiedPurchase == filter.IsVerifiedPurchase.Value);

            if (filter.HasReply.HasValue)
            {
                if (filter.HasReply.Value)
                    query = query.Where(r => r.BusinessReply != null);
                else
                    query = query.Where(r => r.BusinessReply == null);
            }

            if (filter.StartDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.StartDate.Value.ToUniversalTime());

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.EndDate.Value.ToUniversalTime());

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var search = filter.SearchQuery.ToLower();
                query = query.Where(r => 
                    r.Title.ToLower().Contains(search) ||
                    r.Content.ToLower().Contains(search) ||
                    r.AuthorName.ToLower().Contains(search) ||
                    r.AuthorEmail.ToLower().Contains(search));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "rating" => filter.SortDescending ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating),
                "helpful" => filter.SortDescending ? query.OrderByDescending(r => r.HelpfulCount) : query.OrderBy(r => r.HelpfulCount),
                "likes" => filter.SortDescending ? query.OrderByDescending(r => r.LikesCount) : query.OrderBy(r => r.LikesCount),
                _ => filter.SortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
            };

            // Apply pagination
            var skip = (filter.Page - 1) * filter.PageSize;
            var reviews = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            // Get statistics
            var statistics = await GetStatisticsAsync(companyId, filter.ProductId, filter.RoomId);

            // Map to DTOs
            var reviewDtos = reviews.Select(r => MapToDto(r)).ToList();

            return new ReviewListResponseDto
            {
                Reviews = reviewDtos,
                Statistics = statistics,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int companyId, int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .Include(r => r.Media)
                .Include(r => r.RepliedBy)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            return review != null ? MapToDto(review) : null;
        }

        public async Task<ReviewDto> CreateReviewAsync(int companyId, CreateReviewDto dto)
        {
            // Validate that either ProductId or RoomId is provided (but not both)
            if ((dto.ProductId == null && dto.RoomId == null) || 
                (dto.ProductId != null && dto.RoomId != null))
            {
                throw new ArgumentException("A review must be for either a Product or a Room, not both or neither.");
            }

            var review = new Review
            {
                CompanyId = companyId,
                ProductId = dto.ProductId,
                RoomId = dto.RoomId,
                CustomerId = dto.CustomerId,
                AuthorName = dto.AuthorName,
                AuthorEmail = dto.AuthorEmail,
                Country = dto.Country ?? "",
                Title = dto.Title ?? "",
                Content = dto.Content,
                Rating = dto.Rating,
                Source = dto.Source,
                IsVerifiedPurchase = dto.IsVerifiedPurchase,
                OriginalLanguage = dto.OriginalLanguage,
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update statistics
            await UpdateStatisticsAsync(companyId, dto.ProductId, dto.RoomId);
            // Also update global company statistics
            await UpdateStatisticsAsync(companyId, null, null);
            await _cacheService.InvalidateCompanyCacheAsync(companyId);

            return await GetReviewByIdAsync(companyId, review.Id) ?? throw new InvalidOperationException("Failed to retrieve created review");
        }

        public async Task<ReviewDto> UpdateReviewAsync(int companyId, int reviewId, UpdateReviewDto dto)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found");

            if (!string.IsNullOrWhiteSpace(dto.AuthorName))
                review.AuthorName = dto.AuthorName;

            if (!string.IsNullOrWhiteSpace(dto.Country))
                review.Country = dto.Country;

            if (!string.IsNullOrWhiteSpace(dto.Title))
                review.Title = dto.Title;

            if (!string.IsNullOrWhiteSpace(dto.Content))
                review.Content = dto.Content;

            if (dto.Rating.HasValue)
                review.Rating = dto.Rating.Value;

            if (dto.Status.HasValue)
                review.Status = dto.Status.Value;

            if (dto.IsPinned.HasValue)
                review.IsPinned = dto.IsPinned.Value;

            if (!string.IsNullOrWhiteSpace(dto.TranslatedTitle))
                review.TranslatedTitle = dto.TranslatedTitle;

            if (!string.IsNullOrWhiteSpace(dto.TranslatedContent))
                review.TranslatedContent = dto.TranslatedContent;

            review.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update statistics if rating changed
            if (dto.Rating.HasValue)
            {
                await UpdateStatisticsAsync(companyId, review.ProductId, review.RoomId);
                await UpdateStatisticsAsync(companyId, null, null);
            }

            // Any update might affect rendered content
            await _cacheService.InvalidateCompanyCacheAsync(companyId);

            return await GetReviewByIdAsync(companyId, reviewId) ?? throw new InvalidOperationException("Failed to retrieve updated review");
        }

        public async Task<bool> DeleteReviewAsync(int companyId, int reviewId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            if (review == null)
                return false;

            var productId = review.ProductId;
            var roomId = review.RoomId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Update statistics
            await UpdateStatisticsAsync(companyId, productId, roomId);
            await UpdateStatisticsAsync(companyId, null, null);
            await _cacheService.InvalidateCompanyCacheAsync(companyId);

            return true;
        }

        public async Task<ReviewDto> ApproveReviewAsync(int companyId, int reviewId, int userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found");

            review.Status = ReviewStatus.Approved;
            review.ApprovedAt = DateTime.UtcNow;
            review.ApprovedByUserId = userId;
            review.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update statistics
            await UpdateStatisticsAsync(companyId, review.ProductId, review.RoomId);
            await UpdateStatisticsAsync(companyId, null, null);
            await _cacheService.InvalidateCompanyCacheAsync(companyId);

            return await GetReviewByIdAsync(companyId, reviewId) ?? throw new InvalidOperationException("Failed to retrieve approved review");
        }

        public async Task<ReviewDto> RejectReviewAsync(int companyId, int reviewId, int userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found");

            review.Status = ReviewStatus.Rejected;
            review.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update statistics
            await UpdateStatisticsAsync(companyId, review.ProductId, review.RoomId);
            await UpdateStatisticsAsync(companyId, null, null);
            await _cacheService.InvalidateCompanyCacheAsync(companyId);

            return await GetReviewByIdAsync(companyId, reviewId) ?? throw new InvalidOperationException("Failed to retrieve rejected review");
        }

        public async Task<ReviewDto> ReplyToReviewAsync(int companyId, int reviewId, ReviewReplyDto dto, int userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found");

            review.BusinessReply = dto.Reply;
            review.BusinessReplyDate = DateTime.UtcNow;
            review.RepliedByUserId = userId;
            review.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetReviewByIdAsync(companyId, reviewId) ?? throw new InvalidOperationException("Failed to retrieve review with reply");
        }

        public async Task<bool> PinReviewAsync(int companyId, int reviewId, bool isPinned)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            if (review == null)
                return false;

            review.IsPinned = isPinned;
            review.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> BulkActionAsync(int companyId, BulkReviewActionDto dto, int userId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.CompanyId == companyId && dto.ReviewIds.Contains(r.Id))
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            switch (dto.Action.ToLower())
            {
                case "approve":
                    foreach (var review in reviews)
                    {
                        review.Status = ReviewStatus.Approved;
                        review.ApprovedAt = DateTime.UtcNow;
                        review.ApprovedByUserId = userId;
                        review.ModifiedAt = DateTime.UtcNow;
                    }
                    break;

                case "reject":
                    foreach (var review in reviews)
                    {
                        review.Status = ReviewStatus.Rejected;
                        review.ModifiedAt = DateTime.UtcNow;
                    }
                    break;

                case "hide":
                    foreach (var review in reviews)
                    {
                        review.Status = ReviewStatus.Hidden;
                        review.ModifiedAt = DateTime.UtcNow;
                    }
                    break;

                case "delete":
                    _context.Reviews.RemoveRange(reviews);
                    break;

                default:
                    return 0;
            }

            await _context.SaveChangesAsync();

            // Update statistics for affected products and rooms
            var productIds = reviews.Where(r => r.ProductId.HasValue).Select(r => r.ProductId.Value).Distinct();
            var roomIds = reviews.Where(r => r.RoomId.HasValue).Select(r => r.RoomId.Value).Distinct();
            
            foreach (var productId in productIds)
            {
                await UpdateStatisticsAsync(companyId, productId, null);
            }
            
            foreach (var roomId in roomIds)
            {
                await UpdateStatisticsAsync(companyId, null, roomId);
            }

            // Always update global company statistics
            await UpdateStatisticsAsync(companyId, null, null);

            return reviews.Count;
        }

        public async Task<bool> AddInteractionAsync(int companyId, int reviewId, ReviewInteractionDto dto, int? customerId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            if (review == null)
                return false;

            // Check if interaction already exists
            var existingInteraction = await _context.ReviewInteractions
                .FirstOrDefaultAsync(i => i.ReviewId == reviewId &&
                    ((customerId.HasValue && i.CustomerId == customerId) ||
                     (!customerId.HasValue && i.SessionId == dto.SessionId)) &&
                    i.Type == dto.Type);

            if (existingInteraction != null)
                return false;

            var interaction = new ReviewInteraction
            {
                ReviewId = reviewId,
                CustomerId = customerId,
                SessionId = dto.SessionId,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            };

            _context.ReviewInteractions.Add(interaction);

            // Update counts
            switch (dto.Type)
            {
                case InteractionType.Like:
                    review.LikesCount++;
                    break;
                case InteractionType.Dislike:
                    review.DislikesCount++;
                    break;
                case InteractionType.Helpful:
                    review.HelpfulCount++;
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveInteractionAsync(int companyId, int reviewId, InteractionType type, int? customerId, string? sessionId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);

            if (review == null)
                return false;

            var interaction = await _context.ReviewInteractions
                .FirstOrDefaultAsync(i => i.ReviewId == reviewId &&
                    ((customerId.HasValue && i.CustomerId == customerId) ||
                     (!customerId.HasValue && i.SessionId == sessionId)) &&
                    i.Type == type);

            if (interaction == null)
                return false;

            _context.ReviewInteractions.Remove(interaction);

            // Update counts
            switch (type)
            {
                case InteractionType.Like:
                    review.LikesCount = Math.Max(0, review.LikesCount - 1);
                    break;
                case InteractionType.Dislike:
                    review.DislikesCount = Math.Max(0, review.DislikesCount - 1);
                    break;
                case InteractionType.Helpful:
                    review.HelpfulCount = Math.Max(0, review.HelpfulCount - 1);
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ReviewStatisticsDto> GetStatisticsAsync(int companyId, int? productId = null, int? roomId = null)
        {
            var stats = await _context.ReviewStatistics
                .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.ProductId == productId && s.RoomId == roomId);

            if (stats == null)
            {
                await UpdateStatisticsAsync(companyId, productId, roomId);
                stats = await _context.ReviewStatistics
                    .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.ProductId == productId && s.RoomId == roomId);
            }

            if (stats == null)
            {
                // If still no stats, calculate them directly from approved reviews
                var approvedReviews = await _context.Reviews
                    .Where(r => r.CompanyId == companyId && 
                           (r.ProductId == productId || productId == null) &&
                           (r.RoomId == roomId || roomId == null) &&
                           ((int)r.Status == 1 || r.Status == ReviewStatus.Approved))
                    .ToListAsync();

                if (approvedReviews.Any())
                {
                    var distribution = new Dictionary<int, int> { {5,0}, {4,0}, {3,0}, {2,0}, {1,0} };
                    foreach (var review in approvedReviews)
                    {
                        if (distribution.ContainsKey(review.Rating))
                            distribution[review.Rating]++;
                    }

                    var positiveCount = approvedReviews.Count(r => r.Rating >= 4);
                    return new ReviewStatisticsDto
                    {
                        TotalReviews = approvedReviews.Count,
                        AverageRating = (decimal)approvedReviews.Average(r => r.Rating),
                        RatingDistribution = distribution,
                        PositiveReviewsCount = positiveCount,
                        PositivePercentage = approvedReviews.Count > 0 ? (positiveCount * 100m) / approvedReviews.Count : 0,
                        NewReviewsThisWeek = approvedReviews.Count(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                        WeeklyGrowthPercentage = 0,
                        WeeklyTrend = new List<int> { 0, 0, 0, 0, 0, 0, 0 }
                    };
                }

                return new ReviewStatisticsDto
                {
                    TotalReviews = 0,
                    AverageRating = 0,
                    RatingDistribution = new Dictionary<int, int> { {5,0}, {4,0}, {3,0}, {2,0}, {1,0} },
                    PositiveReviewsCount = 0,
                    PositivePercentage = 0
                };
            }

            var weeklyTrend = new List<int>();
            if (!string.IsNullOrEmpty(stats.WeeklyTrend))
            {
                try
                {
                    weeklyTrend = JsonSerializer.Deserialize<List<int>>(stats.WeeklyTrend) ?? new List<int>();
                }
                catch { }
            }

            return new ReviewStatisticsDto
            {
                TotalReviews = stats.TotalReviews,
                AverageRating = stats.AverageRating,
                RatingDistribution = new Dictionary<int, int>
                {
                    { 5, stats.FiveStarCount },
                    { 4, stats.FourStarCount },
                    { 3, stats.ThreeStarCount },
                    { 2, stats.TwoStarCount },
                    { 1, stats.OneStarCount }
                },
                PositiveReviewsCount = stats.PositiveReviewsCount,
                PositivePercentage = stats.PositivePercentage,
                NewReviewsThisWeek = stats.NewReviewsThisWeek,
                WeeklyGrowthPercentage = stats.WeeklyGrowthPercentage,
                WeeklyTrend = weeklyTrend,
                LastCalculatedAt = stats.LastCalculatedAt
            };
        }

        public async Task UpdateStatisticsAsync(int companyId, int? productId = null, int? roomId = null)
        {
            // Accept both Approved enum value and numeric value 1
            var query = _context.Reviews
                .Where(r => r.CompanyId == companyId && (r.Status == ReviewStatus.Approved || (int)r.Status == 1));

            if (productId.HasValue)
                query = query.Where(r => r.ProductId == productId.Value);
                
            if (roomId.HasValue)
                query = query.Where(r => r.RoomId == roomId.Value);

            var reviews = await query.ToListAsync();

            var stats = await _context.ReviewStatistics
                .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.ProductId == productId && s.RoomId == roomId);

            if (stats == null)
            {
                stats = new ReviewStatistics
                {
                    CompanyId = companyId,
                    ProductId = productId,
                    RoomId = roomId
                };
                _context.ReviewStatistics.Add(stats);
            }

            stats.TotalReviews = reviews.Count;
            stats.FiveStarCount = reviews.Count(r => r.Rating == 5);
            stats.FourStarCount = reviews.Count(r => r.Rating == 4);
            stats.ThreeStarCount = reviews.Count(r => r.Rating == 3);
            stats.TwoStarCount = reviews.Count(r => r.Rating == 2);
            stats.OneStarCount = reviews.Count(r => r.Rating == 1);

            if (stats.TotalReviews > 0)
            {
                stats.AverageRating = (decimal)reviews.Average(r => r.Rating);
                stats.PositiveReviewsCount = stats.FiveStarCount + stats.FourStarCount;
                stats.PositivePercentage = (stats.PositiveReviewsCount * 100m) / stats.TotalReviews;
            }
            else
            {
                stats.AverageRating = 0;
                stats.PositiveReviewsCount = 0;
                stats.PositivePercentage = 0;
            }

            // Calculate weekly stats
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            stats.NewReviewsThisWeek = reviews.Count(r => r.CreatedAt >= weekAgo);

            // Calculate weekly trend (reviews per day for last 7 days)
            var weeklyTrend = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var count = reviews.Count(r => r.CreatedAt.Date == date);
                weeklyTrend.Add(count);
            }
            stats.WeeklyTrend = JsonSerializer.Serialize(weeklyTrend);

            stats.LastCalculatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<byte[]> ExportReviewsAsync(int companyId, ReviewFilterDto filter, string format)
        {
            var response = await GetReviewsAsync(companyId, filter);
            var reviews = response.Reviews;

            switch (format.ToLower())
            {
                case "csv":
                    return ExportToCsv(reviews);
                case "excel":
                    return ExportToExcel(reviews);
                default:
                    throw new ArgumentException($"Unsupported export format: {format}");
            }
        }

        private byte[] ExportToCsv(List<ReviewDto> reviews)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID,Product,Author Name,Author Email,Rating,Title,Content,Status,Created Date");

            foreach (var review in reviews)
            {
                csv.AppendLine($"{review.Id},\"{review.Product?.Name ?? ""}\",\"{review.AuthorName}\",\"{review.AuthorEmail}\",{review.Rating},\"{review.Title}\",\"{review.Content.Replace("\"", "\"\"")}\",{review.Status},{review.CreatedAt:yyyy-MM-dd}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private byte[] ExportToExcel(List<ReviewDto> reviews)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='UTF-8'></head><body>");
            html.AppendLine("<table border='1'>");
            html.AppendLine("<tr><th>ID</th><th>Product</th><th>Author Name</th><th>Author Email</th><th>Rating</th><th>Title</th><th>Content</th><th>Status</th><th>Created Date</th></tr>");

            foreach (var review in reviews)
            {
                html.AppendLine($"<tr><td>{review.Id}</td><td>{review.Product?.Name ?? ""}</td><td>{review.AuthorName}</td><td>{review.AuthorEmail}</td><td>{review.Rating}</td><td>{review.Title}</td><td>{review.Content}</td><td>{review.Status}</td><td>{review.CreatedAt:yyyy-MM-dd}</td></tr>");
            }

            html.AppendLine("</table></body></html>");

            return Encoding.UTF8.GetBytes(html.ToString());
        }

        public async Task<ReviewMediaDto> AddMediaAsync(int companyId, int reviewId, string mediaUrl, string? caption, string? thumbnailUrl = null)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyId == companyId);
            if (review == null)
            {
                throw new KeyNotFoundException($"Review with ID {reviewId} not found");
            }

            var media = new ReviewMedia
            {
                ReviewId = reviewId,
                MediaType = "image",
                MediaUrl = mediaUrl,
                ThumbnailUrl = thumbnailUrl ?? mediaUrl,
                Caption = caption,
                SortOrder = 0,
                UploadedAt = DateTime.UtcNow
            };

            _context.Add(media);
            await _context.SaveChangesAsync();

            return new ReviewMediaDto
            {
                Id = media.Id,
                MediaType = media.MediaType,
                MediaUrl = media.MediaUrl,
                ThumbnailUrl = media.ThumbnailUrl,
                Caption = media.Caption,
                SortOrder = media.SortOrder
            };
        }

        private ReviewDto MapToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                CompanyId = review.CompanyId,
                ProductId = review.ProductId,
                RoomId = review.RoomId,
                CustomerId = review.CustomerId,
                AuthorName = review.AuthorName,
                AuthorEmail = review.AuthorEmail,
                Country = review.Country,
                Title = review.Title,
                Content = review.Content,
                Rating = review.Rating,
                Status = review.Status,
                Source = review.Source,
                IsPinned = review.IsPinned,
                IsVerifiedPurchase = review.IsVerifiedPurchase,
                BusinessReply = review.BusinessReply,
                BusinessReplyDate = review.BusinessReplyDate,
                RepliedByUserId = review.RepliedByUserId,
                RepliedByUserName = review.RepliedBy?.FullName,
                LikesCount = review.LikesCount,
                DislikesCount = review.DislikesCount,
                HelpfulCount = review.HelpfulCount,
                CreatedAt = review.CreatedAt,
                ApprovedAt = review.ApprovedAt,
                ModifiedAt = review.ModifiedAt,
                OriginalLanguage = review.OriginalLanguage,
                TranslatedTitle = review.TranslatedTitle,
                TranslatedContent = review.TranslatedContent,
                Product = review.Product != null ? new ProductInfoDto
                {
                    Id = review.Product.Id,
                    Name = review.Product.Name,
                    ImageUrl = review.Product.Images?.FirstOrDefault(),
                    Price = review.Product.BasePrice
                } : null,
                Room = review.Room != null ? new RoomInfoDto
                {
                    Id = review.Room.Id,
                    Name = review.Room.Name,
                    ImageUrl = review.Room.Images?.FirstOrDefault(),
                    Price = review.Room.BasePrice
                } : null,
                Customer = review.Customer != null ? new CustomerInfoDto
                {
                    Id = review.Customer.Id,
                    FullName = review.Customer.FullName,
                    Email = review.Customer.Email,
                    AvatarUrl = review.Customer.Avatar
                } : null,
                Media = review.Media.Select(m => new ReviewMediaDto
                {
                    Id = m.Id,
                    MediaType = m.MediaType,
                    MediaUrl = m.MediaUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    Caption = m.Caption,
                    SortOrder = m.SortOrder
                }).ToList()
            };
        }
    }
}