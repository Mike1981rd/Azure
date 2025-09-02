using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface IReviewService
    {
        // CRUD Operations
        Task<ReviewListResponseDto> GetReviewsAsync(int companyId, ReviewFilterDto filter);
        Task<ReviewDto?> GetReviewByIdAsync(int companyId, int reviewId);
        Task<ReviewDto> CreateReviewAsync(int companyId, CreateReviewDto dto);
        Task<ReviewDto> UpdateReviewAsync(int companyId, int reviewId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(int companyId, int reviewId);
        
        // Review Management
        Task<ReviewDto> ApproveReviewAsync(int companyId, int reviewId, int userId);
        Task<ReviewDto> RejectReviewAsync(int companyId, int reviewId, int userId);
        Task<ReviewDto> ReplyToReviewAsync(int companyId, int reviewId, ReviewReplyDto dto, int userId);
        Task<bool> PinReviewAsync(int companyId, int reviewId, bool isPinned);
        
        // Bulk Operations
        Task<int> BulkActionAsync(int companyId, BulkReviewActionDto dto, int userId);
        
        // Interactions
        Task<bool> AddInteractionAsync(int companyId, int reviewId, ReviewInteractionDto dto, int? customerId);
        Task<bool> RemoveInteractionAsync(int companyId, int reviewId, InteractionType type, int? customerId, string? sessionId);
        
        // Statistics
        Task<ReviewStatisticsDto> GetStatisticsAsync(int companyId, int? productId = null, int? roomId = null);
        Task UpdateStatisticsAsync(int companyId, int? productId = null, int? roomId = null);
        
        // Export
        Task<byte[]> ExportReviewsAsync(int companyId, ReviewFilterDto filter, string format);

        // Media
        Task<ReviewMediaDto> AddMediaAsync(int companyId, int reviewId, string mediaUrl, string? caption, string? thumbnailUrl = null);
    }
}