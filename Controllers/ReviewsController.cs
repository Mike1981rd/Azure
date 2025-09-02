using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services;
using Microsoft.AspNetCore.Http;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IUploadService _uploadService;

        public ReviewsController(IReviewService reviewService, IUploadService uploadService)
        {
            _reviewService = reviewService;
            _uploadService = uploadService;
        }

        private int GetCompanyId()
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
            {
                return 1; // Fallback to default company
            }
            return companyId;
        }

        // Upload media for a review (public; tied to moderation via status)
        [HttpPost("{id}/media")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadReviewMedia(int id, IFormFile file, [FromForm] string? caption = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file provided" });

                // Validate type and size similar to UploadController
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest(new { error = "File must be an image (JPEG, PNG, GIF or WebP)" });

                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { error = "File size must not exceed 5MB" });

                // Save image and create media entity
                var imageUrl = await _uploadService.UploadImageAsync(file);

                // Persist ReviewMedia
                // We need company id to validate ownership for authenticated calls; for public, fallback to 1
                var companyId = User?.Identity?.IsAuthenticated == true ? GetCompanyId() : 1;

                // Persist using service
                var result = await _reviewService.AddMediaAsync(companyId, id, imageUrl, caption, imageUrl);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Review not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while uploading review media", details = ex.Message });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return 0;
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] ReviewFilterDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _reviewService.GetReviewsAsync(companyId, filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching reviews", details = ex.Message });
            }
        }

        // Public endpoint to retrieve approved reviews and statistics for preview/live site
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicReviews([FromQuery] int? productId = null, [FromQuery] int? roomId = null)
        {
            try
            {
                // In production, resolve companyId from domain/host header
                var companyId = 1;

                var filter = new ReviewFilterDto
                {
                    ProductId = productId,
                    RoomId = roomId,
                    Status = ReviewStatus.Approved,
                    SortBy = "CreatedAt",
                    SortDescending = true,
                    Page = 1,
                    PageSize = 100
                };

                var result = await _reviewService.GetReviewsAsync(companyId, filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching public reviews", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var review = await _reviewService.GetReviewByIdAsync(companyId, id);
                
                if (review == null)
                    return NotFound(new { error = "Review not found" });
                
                return Ok(review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching the review", details = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous] // Allow customers to submit reviews without authentication
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            // Validate that either ProductId or RoomId is provided (but not both)
            if ((dto.ProductId == null && dto.RoomId == null) || 
                (dto.ProductId != null && dto.RoomId != null))
            {
                return BadRequest(new { error = "A review must be for either a Product or a Room, not both or neither." });
            }

            try
            {
                // For public reviews, get company ID from a different source (e.g., domain or header)
                var companyId = 1; // In production, this should be resolved from domain or header
                
                var review = await _reviewService.CreateReviewAsync(companyId, dto);
                return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating the review", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var companyId = GetCompanyId();
                var review = await _reviewService.UpdateReviewAsync(companyId, id, dto);
                return Ok(review);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Review not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating the review", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _reviewService.DeleteReviewAsync(companyId, id);
                
                if (!result)
                    return NotFound(new { error = "Review not found" });
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the review", details = ex.Message });
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveReview(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var userId = GetUserId();
                var review = await _reviewService.ApproveReviewAsync(companyId, id, userId);
                return Ok(review);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Review not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while approving the review", details = ex.Message });
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectReview(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var userId = GetUserId();
                var review = await _reviewService.RejectReviewAsync(companyId, id, userId);
                return Ok(review);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Review not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while rejecting the review", details = ex.Message });
            }
        }

        [HttpPost("{id}/reply")]
        public async Task<IActionResult> ReplyToReview(int id, [FromBody] ReviewReplyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var companyId = GetCompanyId();
                var userId = GetUserId();
                var review = await _reviewService.ReplyToReviewAsync(companyId, id, dto, userId);
                return Ok(review);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Review not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while replying to the review", details = ex.Message });
            }
        }

        [HttpPost("{id}/pin")]
        public async Task<IActionResult> PinReview(int id, [FromQuery] bool isPinned = true)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _reviewService.PinReviewAsync(companyId, id, isPinned);
                
                if (!result)
                    return NotFound(new { error = "Review not found" });
                
                return Ok(new { success = true, isPinned });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while pinning the review", details = ex.Message });
            }
        }

        [HttpPost("bulk-action")]
        public async Task<IActionResult> BulkAction([FromBody] BulkReviewActionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var companyId = GetCompanyId();
                var userId = GetUserId();
                var count = await _reviewService.BulkActionAsync(companyId, dto, userId);
                return Ok(new { success = true, affectedCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while performing bulk action", details = ex.Message });
            }
        }

        [HttpPost("recalculate-statistics")]
        public async Task<IActionResult> RecalculateStatistics()
        {
            try
            {
                var companyId = GetCompanyId();
                await _reviewService.UpdateStatisticsAsync(companyId);
                return Ok(new { success = true, message = "Statistics recalculated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while recalculating statistics", details = ex.Message });
            }
        }

        [HttpPost("{id}/interact")]
        [AllowAnonymous] // Allow public interaction
        public async Task<IActionResult> AddInteraction(int id, [FromBody] ReviewInteractionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var companyId = 1; // In production, resolve from domain
                int? customerId = null;
                
                // If authenticated, get customer ID
                if (User.Identity?.IsAuthenticated == true)
                {
                    var customerClaim = User.FindFirst("customerId")?.Value;
                    if (!string.IsNullOrEmpty(customerClaim) && int.TryParse(customerClaim, out int cId))
                    {
                        customerId = cId;
                    }
                }

                var result = await _reviewService.AddInteractionAsync(companyId, id, dto, customerId);
                
                if (!result)
                    return BadRequest(new { error = "Interaction already exists or review not found" });
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while adding interaction", details = ex.Message });
            }
        }

        [HttpDelete("{id}/interact")]
        [AllowAnonymous] // Allow public interaction
        public async Task<IActionResult> RemoveInteraction(int id, [FromQuery] InteractionType type, [FromQuery] string? sessionId = null)
        {
            try
            {
                var companyId = 1; // In production, resolve from domain
                int? customerId = null;
                
                // If authenticated, get customer ID
                if (User.Identity?.IsAuthenticated == true)
                {
                    var customerClaim = User.FindFirst("customerId")?.Value;
                    if (!string.IsNullOrEmpty(customerClaim) && int.TryParse(customerClaim, out int cId))
                    {
                        customerId = cId;
                    }
                }

                var result = await _reviewService.RemoveInteractionAsync(companyId, id, type, customerId, sessionId);
                
                if (!result)
                    return NotFound(new { error = "Interaction not found" });
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while removing interaction", details = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] int? productId = null, [FromQuery] int? roomId = null)
        {
            try
            {
                var companyId = GetCompanyId();
                var statistics = await _reviewService.GetStatisticsAsync(companyId, productId, roomId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching statistics", details = ex.Message });
            }
        }

        [HttpPost("statistics/update")]
        public async Task<IActionResult> UpdateStatistics([FromQuery] int? productId = null, [FromQuery] int? roomId = null)
        {
            try
            {
                var companyId = GetCompanyId();
                await _reviewService.UpdateStatisticsAsync(companyId, productId, roomId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating statistics", details = ex.Message });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportReviews([FromQuery] ReviewFilterDto filter, [FromQuery] string format = "csv")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _reviewService.ExportReviewsAsync(companyId, filter, format);
                
                var contentType = format.ToLower() == "csv" 
                    ? "text/csv" 
                    : "application/vnd.ms-excel";
                    
                var fileName = $"reviews_{DateTime.Now:yyyyMMdd}.{format.ToLower()}";
                
                return File(data, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while exporting reviews", details = ex.Message });
            }
        }
    }
}