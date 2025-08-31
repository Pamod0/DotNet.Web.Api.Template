using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.Web.Api.Template.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("User/{id}")]
        public async Task<IActionResult> GetPersistentNotificationsForUsers(Guid id)
        {
            try
            {
                var notifications = await _notificationService.GetPersistentNotificationsForUsersAsync(id);
                if (notifications == null || !notifications.Any())
                {
                    return NotFound("No notifications found for the specified user.");
                }

                //return Ok(notifications);
                // Convert UTC times to Sri Lanka time for frontend display
                var notificationsWithSriLankaTime = notifications.Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.RecipientUserId,
                    n.RecipientDepartmentId,
                    n.RelatedEntityId,
                    n.RelatedEntityType,
                    n.IsRead,
                    SentAt = ConvertToSriLankaTime(n.SentAt),
                    CreatedAt = ConvertToSriLankaTime(n.CreatedAt),
                    UpdatedAt = n.UpdatedAt.HasValue ? ConvertToSriLankaTime(n.UpdatedAt.Value) : (DateTime?)null
                });

                return Ok(notificationsWithSriLankaTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching persistent notifications for user {userId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("Department/{id}")]
        public async Task<IActionResult> GetPersistentNotificationsForDepartments(Guid id)
        {
            try
            {
                var notifications = await _notificationService.GetPersistentNotificationsAsync(id);
                if (notifications == null || !notifications.Any())
                {
                    return NotFound("No notifications found for the specified department.");
                }

                // Convert UTC times to Sri Lanka time for frontend display
                var notificationsWithSriLankaTime = notifications.Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.RecipientUserId,
                    n.RecipientDepartmentId,
                    n.RelatedEntityId,
                    n.RelatedEntityType,
                    n.IsRead,
                    SentAt = ConvertToSriLankaTime(n.SentAt),
                    CreatedAt = ConvertToSriLankaTime(n.CreatedAt),
                    UpdatedAt = n.UpdatedAt.HasValue ? ConvertToSriLankaTime(n.UpdatedAt.Value) : (DateTime?)null
                });

                return Ok(notificationsWithSriLankaTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching persistent notifications for department {DepartmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUnreadNotificationCount(Guid id)
        {
            try
            {
                var unreadCount = await _notificationService.GetUnreadNotificationCountAsync(id);
                return Ok(unreadCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching unread notification count for department {DepartmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPut("MarkAsRead/{notificationId}")]
        public async Task<IActionResult> SetNotificationAsRead(Guid notificationId)
        {
            try
            {
                await _notificationService.SetNotificationAsReadAsync(notificationId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Notification with ID {NotificationId} not found.", notificationId);
                return NotFound("Notification not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting notification {NotificationId} as read.", notificationId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        private DateTime ConvertToSriLankaTime(DateTime utcTime)
        {
            try
            {
                var sriLankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo");
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, sriLankaTimeZone);
            }
            catch
            {
                // Fallback: Sri Lanka is UTC+5:30
                return utcTime.AddHours(5.5);
            }
        }

    }
}
