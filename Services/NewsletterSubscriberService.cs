using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.NewsletterSubscribers;
using WebsiteBuilderAPI.DTOs.Customers;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class NewsletterSubscriberService : INewsletterSubscriberService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomerService _customerService;

        public NewsletterSubscriberService(ApplicationDbContext context, ICustomerService customerService)
        {
            _context = context;
            _customerService = customerService;
        }

        public async Task<PagedResult<NewsletterSubscriberDto>> GetPagedAsync(int companyId, int page = 1, int pageSize = 20, string status = "", string search = "")
        {
            var query = _context.NewsletterSubscribers
                .Where(ns => ns.CompanyId == companyId && ns.DeletedAt == null);

            // Status filter
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "active":
                        query = query.Where(ns => ns.IsActive);
                        break;
                    case "inactive":
                        query = query.Where(ns => !ns.IsActive);
                        break;
                    case "converted":
                        query = query.Where(ns => ns.ConvertedToCustomer);
                        break;
                    case "pending":
                        query = query.Where(ns => ns.IsActive && !ns.ConvertedToCustomer);
                        break;
                }
            }

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(ns =>
                    ns.Email.Contains(search) ||
                    ns.FirstName.Contains(search) ||
                    ns.LastName.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var subscribers = await query
                .OrderByDescending(ns => ns.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ns => MapToDto(ns))
                .ToListAsync();

            return new PagedResult<NewsletterSubscriberDto>
            {
                Items = subscribers,
                TotalCount = totalCount,
                Page = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
        }

        public async Task<NewsletterSubscriberDto> GetByIdAsync(int companyId, int id)
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(ns => ns.Id == id && ns.CompanyId == companyId && ns.DeletedAt == null);

            if (subscriber == null)
                throw new KeyNotFoundException("Newsletter subscriber not found");

            return MapToDto(subscriber);
        }

        public async Task<NewsletterSubscriberDto> CreateAsync(int companyId, CreateNewsletterSubscriberDto dto)
        {
            // Check for existing subscriber
            var existing = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(ns => ns.CompanyId == companyId && ns.Email == dto.Email && ns.DeletedAt == null);

            if (existing != null)
                throw new InvalidOperationException("Email already subscribed");

            var subscriber = new NewsletterSubscriber
            {
                CompanyId = companyId,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                SourcePage = dto.SourcePage ?? "admin", // Default when created manually
                SourceCampaign = dto.SourceCampaign ?? "manual",
                Language = dto.Language ?? "es",
                AcceptedMarketing = dto.AcceptedMarketing,
                AcceptedTerms = dto.AcceptedTerms,
                OptInDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.NewsletterSubscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            return MapToDto(subscriber);
        }

        public async Task<NewsletterSubscriberDto> UpdateAsync(int companyId, int id, UpdateNewsletterSubscriberDto dto)
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(ns => ns.Id == id && ns.CompanyId == companyId && ns.DeletedAt == null);

            if (subscriber == null)
                throw new KeyNotFoundException("Newsletter subscriber not found");

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.Email))
            {
                // Check for duplicate email
                var existing = await _context.NewsletterSubscribers
                    .AnyAsync(ns => ns.Id != id && ns.CompanyId == companyId && ns.Email == dto.Email && ns.DeletedAt == null);
                
                if (existing)
                    throw new InvalidOperationException("Email already exists");
                
                subscriber.Email = dto.Email;
            }

            if (dto.FirstName != null)
                subscriber.FirstName = dto.FirstName == "" ? null : dto.FirstName;
            
            if (dto.LastName != null)
                subscriber.LastName = dto.LastName == "" ? null : dto.LastName;
            
            if (dto.Phone != null)
                subscriber.Phone = dto.Phone == "" ? null : dto.Phone;

            if (dto.IsActive.HasValue)
            {
                subscriber.IsActive = dto.IsActive.Value;
                if (!dto.IsActive.Value)
                {
                    subscriber.OptOutDate = DateTime.UtcNow;
                    subscriber.UnsubscribeReason = dto.UnsubscribeReason;
                }
                else
                {
                    subscriber.OptOutDate = null;
                    subscriber.UnsubscribeReason = null;
                }
            }

            if (!string.IsNullOrEmpty(dto.Language))
                subscriber.Language = dto.Language;

            if (dto.AcceptedMarketing.HasValue)
                subscriber.AcceptedMarketing = dto.AcceptedMarketing.Value;

            subscriber.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToDto(subscriber);
        }

        public async Task DeleteAsync(int companyId, int id)
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(ns => ns.Id == id && ns.CompanyId == companyId && ns.DeletedAt == null);

            if (subscriber == null)
                throw new KeyNotFoundException("Newsletter subscriber not found");

            // Soft delete
            subscriber.DeletedAt = DateTime.UtcNow;
            subscriber.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }

        public async Task<NewsletterSubscriberDto> PublicSubscribeAsync(PublicSubscribeDto dto)
        {
            // Default to company ID 1 for single-tenant setup
            int companyId = 1;

            // Check for existing subscriber
            var existing = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(ns => ns.CompanyId == companyId && ns.Email == dto.Email && ns.DeletedAt == null);

            if (existing != null)
            {
                if (existing.IsActive)
                    throw new InvalidOperationException("Email already subscribed");
                
                // Reactivate if previously unsubscribed
                existing.IsActive = true;
                existing.OptInDate = DateTime.UtcNow;
                existing.OptOutDate = null;
                existing.UnsubscribeReason = null;
                existing.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return MapToDto(existing);
            }

            var subscriber = new NewsletterSubscriber
            {
                CompanyId = companyId,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                SourcePage = dto.SourcePage ?? "website", // Default for public subscriptions
                SourceCampaign = dto.SourceCampaign ?? "organic",
                Language = dto.Language ?? "es",
                AcceptedMarketing = dto.AcceptedMarketing,
                AcceptedTerms = dto.AcceptedTerms,
                OptInDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.NewsletterSubscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            return MapToDto(subscriber);
        }

        public async Task<bool> PublicUnsubscribeAsync(string email, int companyId)
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(ns => ns.Email == email && ns.CompanyId == companyId && ns.DeletedAt == null);

            if (subscriber == null || !subscriber.IsActive)
                return false;

            subscriber.IsActive = false;
            subscriber.OptOutDate = DateTime.UtcNow;
            subscriber.UnsubscribeReason = "User requested unsubscribe";
            subscriber.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<NewsletterSubscriberDto> ConvertToCustomerAsync(int companyId, int subscriberId, ConvertToCustomerDto dto)
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(ns => ns.Id == subscriberId && ns.CompanyId == companyId && ns.DeletedAt == null);

            if (subscriber == null)
                throw new KeyNotFoundException("Newsletter subscriber not found");

            if (subscriber.ConvertedToCustomer)
                throw new InvalidOperationException("Subscriber already converted to customer");

            // Create customer using existing CustomerService
            var createCustomerDto = new CreateCustomerDto
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = subscriber.Email, // Use email as username
                Email = subscriber.Email,
                Phone = dto.Phone,
                Country = dto.Country ?? "DO", // Default to Dominican Republic
                Password = dto.Password ?? GenerateRandomPassword(),
                BirthDate = dto.BirthDate?.ToUniversalTime()
            };

            var customer = await _customerService.CreateCustomerAsync(companyId, createCustomerDto);

            // Update subscriber to mark as converted
            subscriber.ConvertedToCustomer = true;
            subscriber.CustomerId = customer.Id;
            subscriber.ConvertedAt = DateTime.UtcNow;
            subscriber.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(subscriber);
        }

        public async Task<bool> ToggleActiveStatusAsync(int companyId, int id, bool isActive)
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(ns => ns.Id == id && ns.CompanyId == companyId && ns.DeletedAt == null);

            if (subscriber == null)
                return false;

            subscriber.IsActive = isActive;
            if (isActive)
            {
                subscriber.OptInDate = DateTime.UtcNow;
                subscriber.OptOutDate = null;
                subscriber.UnsubscribeReason = null;
            }
            else
            {
                subscriber.OptOutDate = DateTime.UtcNow;
                subscriber.UnsubscribeReason = "Status toggled by admin";
            }
            subscriber.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<NewsletterSubscriberDto>> GetRecentSubscribersAsync(int companyId, int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var subscribers = await _context.NewsletterSubscribers
                .Where(ns => ns.CompanyId == companyId && 
                           ns.DeletedAt == null && 
                           ns.CreatedAt >= cutoffDate)
                .OrderByDescending(ns => ns.CreatedAt)
                .Select(ns => MapToDto(ns))
                .ToListAsync();

            return subscribers;
        }

        public async Task<Dictionary<string, object>> GetStatisticsAsync(int companyId)
        {
            var totalActive = await _context.NewsletterSubscribers
                .CountAsync(ns => ns.CompanyId == companyId && ns.IsActive && ns.DeletedAt == null);

            var totalInactive = await _context.NewsletterSubscribers
                .CountAsync(ns => ns.CompanyId == companyId && !ns.IsActive && ns.DeletedAt == null);

            var totalConverted = await _context.NewsletterSubscribers
                .CountAsync(ns => ns.CompanyId == companyId && ns.ConvertedToCustomer && ns.DeletedAt == null);

            var thisMonth = await _context.NewsletterSubscribers
                .CountAsync(ns => ns.CompanyId == companyId && 
                                ns.DeletedAt == null &&
                                ns.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            var stats = new Dictionary<string, object>
            {
                ["totalActive"] = totalActive,
                ["totalInactive"] = totalInactive,
                ["totalConverted"] = totalConverted,
                ["thisMonth"] = thisMonth,
                ["conversionRate"] = totalActive > 0 ? Math.Round((double)totalConverted / totalActive * 100, 2) : 0
            };

            return stats;
        }

        public async Task<List<NewsletterSubscriberDto>> GetByEmailAsync(int companyId, string email)
        {
            var subscribers = await _context.NewsletterSubscribers
                .Where(ns => ns.CompanyId == companyId && ns.Email == email && ns.DeletedAt == null)
                .Select(ns => MapToDto(ns))
                .ToListAsync();

            return subscribers;
        }

        public async Task<List<NewsletterSubscriberDto>> BulkUpdateStatusAsync(int companyId, List<int> subscriberIds, bool isActive)
        {
            var subscribers = await _context.NewsletterSubscribers
                .Where(ns => subscriberIds.Contains(ns.Id) && ns.CompanyId == companyId && ns.DeletedAt == null)
                .ToListAsync();

            foreach (var subscriber in subscribers)
            {
                subscriber.IsActive = isActive;
                if (isActive)
                {
                    subscriber.OptInDate = DateTime.UtcNow;
                    subscriber.OptOutDate = null;
                    subscriber.UnsubscribeReason = null;
                }
                else
                {
                    subscriber.OptOutDate = DateTime.UtcNow;
                    subscriber.UnsubscribeReason = "Bulk update by admin";
                }
                subscriber.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            
            return subscribers.Select(MapToDto).ToList();
        }

        public async Task<int> BulkDeleteAsync(int companyId, List<int> subscriberIds)
        {
            var subscribers = await _context.NewsletterSubscribers
                .Where(ns => subscriberIds.Contains(ns.Id) && ns.CompanyId == companyId && ns.DeletedAt == null)
                .ToListAsync();

            foreach (var subscriber in subscribers)
            {
                subscriber.DeletedAt = DateTime.UtcNow;
                subscriber.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            
            return subscribers.Count;
        }

        // Helper methods
        private static NewsletterSubscriberDto MapToDto(NewsletterSubscriber subscriber)
        {
            return new NewsletterSubscriberDto
            {
                Id = subscriber.Id,
                CompanyId = subscriber.CompanyId,
                Email = subscriber.Email,
                FirstName = subscriber.FirstName,
                LastName = subscriber.LastName,
                FullName = subscriber.FullName,
                Phone = subscriber.Phone,
                IsActive = subscriber.IsActive,
                SourcePage = subscriber.SourcePage,
                SourceCampaign = subscriber.SourceCampaign,
                Language = subscriber.Language,
                AcceptedMarketing = subscriber.AcceptedMarketing,
                AcceptedTerms = subscriber.AcceptedTerms,
                OptInDate = subscriber.OptInDate,
                OptOutDate = subscriber.OptOutDate,
                UnsubscribeReason = subscriber.UnsubscribeReason,
                ConvertedToCustomer = subscriber.ConvertedToCustomer,
                CustomerId = subscriber.CustomerId,
                ConvertedAt = subscriber.ConvertedAt,
                CreatedAt = subscriber.CreatedAt,
                UpdatedAt = subscriber.UpdatedAt,
                DaysSinceSubscription = subscriber.DaysSinceSubscription,
                IsConverted = subscriber.IsConverted
            };
        }

        private static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}