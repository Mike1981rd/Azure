using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services;
using System.Linq;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService service, ILogger<NotificationsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int GetCompanyId()
        {
            var claim = User?.FindFirst("companyId")?.Value;
            if (int.TryParse(claim, out var id)) return id;
            return 1; // single-tenant fallback
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetRecent([FromQuery] int limit = 20)
        {
            var companyId = GetCompanyId();
            var items = await _service.GetRecentAsync(companyId, limit);
            return Ok(items.Select(n => new {
                n.Id, n.Type, n.Title, n.Message, n.IsRead, n.CreatedAt, n.RelatedEntityType, n.RelatedEntityId
            }));
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<object>> GetUnreadCount()
        {
            var companyId = GetCompanyId();
            var count = await _service.GetUnreadCountAsync(companyId);
            return Ok(new { count });
        }

        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var companyId = GetCompanyId();
            var ok = await _service.MarkAsReadAsync(companyId, id);
            if (!ok) return NotFound();
            return Ok(new { success = true });
        }

        [HttpPost("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var companyId = GetCompanyId();
            var total = await _service.MarkAllAsReadAsync(companyId);
            return Ok(new { success = true, updated = total });
        }
    }
}
