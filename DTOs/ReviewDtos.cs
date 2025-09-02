using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.DTOs
{
    public class CreateReviewDto
    {
        // One of these is required (validated in controller)
        public int? ProductId { get; set; }
        public int? RoomId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string AuthorEmail { get; set; } = string.Empty;

        [MaxLength(2)]
        public string? Country { get; set; }

        [MaxLength(500)]
        public string? Title { get; set; }

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string Source { get; set; } = "Customer submission";

        public bool IsVerifiedPurchase { get; set; } = false;

        public string? OriginalLanguage { get; set; }
    }

    public class UpdateReviewDto
    {
        [MaxLength(200)]
        public string? AuthorName { get; set; }

        [MaxLength(2)]
        public string? Country { get; set; }

        [MaxLength(500)]
        public string? Title { get; set; }

        [MaxLength(5000)]
        public string? Content { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        public ReviewStatus? Status { get; set; }

        public bool? IsPinned { get; set; }

        [MaxLength(500)]
        public string? TranslatedTitle { get; set; }

        [MaxLength(5000)]
        public string? TranslatedContent { get; set; }
    }

    public class ReviewReplyDto
    {
        [Required]
        [MaxLength(5000)]
        public string Reply { get; set; } = string.Empty;
    }

    public class ReviewInteractionDto
    {
        [Required]
        public InteractionType Type { get; set; }

        public string? SessionId { get; set; }
    }

    public class ReviewFilterDto
    {
        public int? ProductId { get; set; }
        public int? RoomId { get; set; }
        public ReviewStatus? Status { get; set; }
        public int? Rating { get; set; }
        public bool? IsVerifiedPurchase { get; set; }
        public bool? HasReply { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? ProductId { get; set; }
        public int? RoomId { get; set; }
        public int? CustomerId { get; set; }

        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }

        public ReviewStatus Status { get; set; }
        public string Source { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
        public bool IsVerifiedPurchase { get; set; }

        public string? BusinessReply { get; set; }
        public DateTime? BusinessReplyDate { get; set; }
        public int? RepliedByUserId { get; set; }
        public string? RepliedByUserName { get; set; }

        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int HelpfulCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public string? OriginalLanguage { get; set; }
        public string? TranslatedTitle { get; set; }
        public string? TranslatedContent { get; set; }

        // Related data
        public ProductInfoDto? Product { get; set; }
        public RoomInfoDto? Room { get; set; }
        public CustomerInfoDto? Customer { get; set; }
        public List<ReviewMediaDto> Media { get; set; } = new List<ReviewMediaDto>();
        public bool UserHasInteracted { get; set; }
        public InteractionType? UserInteractionType { get; set; }
    }

    public class ReviewMediaDto
    {
        public int Id { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? Caption { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomerInfoDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    public class RoomInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
    }

    public class ReviewStatisticsDto
    {
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int PositiveReviewsCount { get; set; }
        public decimal PositivePercentage { get; set; }
        public int NewReviewsThisWeek { get; set; }
        public decimal WeeklyGrowthPercentage { get; set; }
        public List<int> WeeklyTrend { get; set; } = new();
        public DateTime LastCalculatedAt { get; set; }
    }

    public class ReviewListResponseDto
    {
        public List<ReviewDto> Reviews { get; set; } = new();
        public ReviewStatisticsDto Statistics { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class BulkReviewActionDto
    {
        [Required]
        public List<int> ReviewIds { get; set; } = new();

        [Required]
        public string Action { get; set; } = string.Empty; // approve, reject, delete, hide
    }
}