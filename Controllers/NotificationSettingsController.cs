using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.DTOs.Notifications;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationSettingsController : ControllerBase
    {
        private readonly INotificationSettingsService _service;
        private readonly ILogger<NotificationSettingsController> _logger;

        public NotificationSettingsController(
            INotificationSettingsService service,
            ILogger<NotificationSettingsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all notification settings for the company
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Get companyId from JWT token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                var settings = await _service.GetAllByCompanyAsync(companyId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification settings");
                return StatusCode(500, new { error = "An error occurred while fetching notification settings" });
            }
        }

        /// <summary>
        /// Get a specific notification setting by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Get companyId from JWT token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                var setting = await _service.GetByIdAsync(companyId, id);
                if (setting == null)
                {
                    return NotFound(new { error = "Notification setting not found" });
                }

                return Ok(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification setting {Id}", id);
                return StatusCode(500, new { error = "An error occurred while fetching the notification setting" });
            }
        }

        /// <summary>
        /// Create a new notification setting
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationSettingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get companyId from JWT token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                var setting = await _service.CreateAsync(companyId, dto);
                return CreatedAtAction(nameof(GetById), new { id = setting.Id }, setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification setting");
                return StatusCode(500, new { error = "An error occurred while creating the notification setting" });
            }
        }

        /// <summary>
        /// Update a notification setting
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationSettingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get companyId from JWT token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                var setting = await _service.UpdateAsync(companyId, id, dto);
                if (setting == null)
                {
                    return NotFound(new { error = "Notification setting not found" });
                }

                return Ok(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification setting {Id}", id);
                return StatusCode(500, new { error = "An error occurred while updating the notification setting" });
            }
        }

        /// <summary>
        /// Delete a notification setting
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get companyId from JWT token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                var deleted = await _service.DeleteAsync(companyId, id);
                if (!deleted)
                {
                    return NotFound(new { error = "Notification setting not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification setting {Id}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the notification setting" });
            }
        }

        /// <summary>
        /// Bulk update notification settings
        /// </summary>
        [HttpPost("bulk-update")]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateNotificationSettingsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get companyId from JWT token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                var settings = await _service.BulkUpdateAsync(companyId, dto);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating notification settings");
                return StatusCode(500, new { error = "An error occurred while updating notification settings" });
            }
        }

        /// <summary>
        /// Initialize default notification settings for the company
        /// </summary>
        [HttpPost("initialize-defaults")]
        public async Task<IActionResult> InitializeDefaults()
        {
            try
            {
                // Get companyId from JWT token
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Default company
                }

                await _service.InitializeDefaultSettingsAsync(companyId);
                var settings = await _service.GetAllByCompanyAsync(companyId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default notification settings");
                return StatusCode(500, new { error = "An error occurred while initializing notification settings" });
            }
        }
    }
}