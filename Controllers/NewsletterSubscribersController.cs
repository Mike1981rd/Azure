using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.NewsletterSubscribers;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NewsletterSubscribersController : ControllerBase
    {
        private readonly INewsletterSubscriberService _subscriberService;

        public NewsletterSubscribersController(INewsletterSubscriberService subscriberService)
        {
            _subscriberService = subscriberService;
        }

        /// <summary>
        /// Get paginated list of newsletter subscribers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSubscribers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string status = "",
            [FromQuery] string search = "")
        {
            try
            {
                // CRITICAL: Use lowercase "companyId" with fallback (from troubleshooting docs)
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _subscriberService.GetPagedAsync(companyId, page, pageSize, status, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Get subscriber by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubscriber(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var subscriber = await _subscriberService.GetByIdAsync(companyId, id);
                return Ok(subscriber);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Newsletter subscriber not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new newsletter subscriber
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSubscriber([FromBody] CreateNewsletterSubscriberDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var subscriber = await _subscriberService.CreateAsync(companyId, dto);
                return CreatedAtAction(nameof(GetSubscriber), new { id = subscriber.Id }, subscriber);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Update existing newsletter subscriber
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubscriber(int id, [FromBody] UpdateNewsletterSubscriberDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var subscriber = await _subscriberService.UpdateAsync(companyId, id, dto);
                return Ok(subscriber);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Newsletter subscriber not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete newsletter subscriber (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscriber(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                await _subscriberService.DeleteAsync(companyId, id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Newsletter subscriber not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Convert subscriber to customer
        /// </summary>
        [HttpPost("{id}/convert-to-customer")]
        public async Task<IActionResult> ConvertToCustomer(int id, [FromBody] ConvertToCustomerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var subscriber = await _subscriberService.ConvertToCustomerAsync(companyId, id, dto);
                return Ok(new { message = "Subscriber converted to customer successfully", subscriber });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Newsletter subscriber not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Toggle subscriber active status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _subscriberService.ToggleActiveStatusAsync(companyId, id, isActive);
                if (!result)
                    return NotFound(new { error = "Newsletter subscriber not found" });

                return Ok(new { message = $"Subscriber {(isActive ? "activated" : "deactivated")} successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Get recent subscribers
        /// </summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentSubscribers([FromQuery] int days = 30)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var subscribers = await _subscriberService.GetRecentSubscribersAsync(companyId, days);
                return Ok(subscribers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Get newsletter statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var stats = await _subscriberService.GetStatisticsAsync(companyId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Bulk update subscriber status
        /// </summary>
        [HttpPatch("bulk/status")]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var subscribers = await _subscriberService.BulkUpdateStatusAsync(companyId, dto.SubscriberIds, dto.IsActive);
                return Ok(new { message = $"Updated {subscribers.Count} subscribers", subscribers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// Bulk delete subscribers
        /// </summary>
        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var deletedCount = await _subscriberService.BulkDeleteAsync(companyId, dto.SubscriberIds);
                return Ok(new { message = $"Deleted {deletedCount} subscribers" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }
    }

    // DTOs for bulk operations
    public class BulkStatusUpdateDto
    {
        public List<int> SubscriberIds { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class BulkDeleteDto
    {
        public List<int> SubscriberIds { get; set; } = new();
    }
}