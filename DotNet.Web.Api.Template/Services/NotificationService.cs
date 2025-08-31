using ASP.NET_Core_Identity.Data;
using ASP.NET_Core_Identity.DTOs.Decision;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Hubs;
using ASP.NET_Core_Identity.Models.Auth;
using ASP.NET_Core_Identity.Models.Notification;
using ASP.NET_Core_Identity.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Core_Identity.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(ApplicationDbContext dbContext, IHubContext<NotificationHub> hubContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        public async Task CreateAndSendMeetingNotification(MeetingDto meeting, IEnumerable<Guid> departmentIds)
        {
            var notificationMessage = $"A new meeting on {meeting.MeetingDate:d} has been created.";
            // Store UTC time in database
            var utcTime = DateTime.UtcNow;

            foreach (var departmentId in departmentIds)
            {
                var usersInDepartment = await _userManager.Users
                    .Where(u => u.DepartmentId == departmentId)
                    .ToListAsync();

                foreach (var user in usersInDepartment)
                {
                    var notification = new Notification
                    {
                        Message = notificationMessage,
                        RecipientUserId = user.Id,
                        RecipientDepartmentId = departmentId,
                        RelatedEntityId = meeting.Id,
                        RelatedEntityType = "Meeting",
                        IsRead = false,
                        SentAt = utcTime, // Store UTC time
                        CreatedAt = utcTime,
                        UpdatedAt = null
                    };
                    _dbContext.Notifications.Add(notification);
                    await _dbContext.SaveChangesAsync();

                    // Send UTC time to frontend, it will convert to Sri Lanka time
                    await _hubContext.Clients.User(user.Id.ToString()).SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        message = notificationMessage,
                        meetingId = meeting.Id,
                        description = meeting.Description ?? string.Empty,
                        isRead = false,
                        sentAt = notification.SentAt, // UTC time
                        createdAt = notification.CreatedAt, // UTC time
                        updatedAt = notification.UpdatedAt
                    });
                }
            }
        }

        public async Task CreateAndSendDecisionNotification(DecisionDto decision, IEnumerable<Guid> departmentIds)
        {
            var decisionName = GetDecisionDisplayName(decision);
            var notificationMessage = $"A new decision '{decisionName}' has been created.";
            // Store UTC time in database
            var utcTime = DateTime.UtcNow;

            foreach (var departmentId in departmentIds)
            {
                var usersInDepartment = await _userManager.Users
                    .Where(u => u.DepartmentId == departmentId)
                    .ToListAsync();

                foreach (var user in usersInDepartment)
                {
                    var notification = new Notification
                    {
                        Message = notificationMessage,
                        RecipientUserId = user.Id,
                        RecipientDepartmentId = departmentId,
                        RelatedEntityId = decision.Id,
                        RelatedEntityType = "Decision",
                        IsRead = false,
                        SentAt = utcTime, // Store UTC time
                        CreatedAt = utcTime,
                        UpdatedAt = null
                    };
                    _dbContext.Notifications.Add(notification);
                    await _dbContext.SaveChangesAsync();

                    // Send UTC time to frontend
                    await _hubContext.Clients.User(user.Id.ToString()).SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        message = notificationMessage,
                        decisionId = decision.Id,
                        description = GetDecisionDescription(decision),
                        isRead = false,
                        sentAt = notification.SentAt, // UTC time
                        createdAt = notification.CreatedAt, // UTC time
                        updatedAt = notification.UpdatedAt
                    });
                }
            }
        }

        public async Task CreateAndSendTaskNotification(DecisionDto decision, IEnumerable<Guid> departmentIds)
        {
            var decisionName = GetDecisionDisplayName(decision);
            var notificationMessage = $"New tasks have been assigned from decision '{decisionName}'.";
            // Store UTC time in database
            var utcTime = DateTime.UtcNow;

            foreach (var departmentId in departmentIds)
            {
                var usersInDepartment = await _userManager.Users
                    .Where(u => u.DepartmentId == departmentId)
                    .ToListAsync();

                foreach (var user in usersInDepartment)
                {
                    var notification = new Notification
                    {
                        Message = notificationMessage,
                        RecipientUserId = user.Id,
                        RecipientDepartmentId = departmentId,
                        RelatedEntityId = decision.Id,
                        RelatedEntityType = "Task",
                        IsRead = false,
                        SentAt = utcTime, // Store UTC time
                        CreatedAt = utcTime,
                        UpdatedAt = null
                    };
                    _dbContext.Notifications.Add(notification);
                    await _dbContext.SaveChangesAsync();

                    // Send UTC time to frontend
                    await _hubContext.Clients.User(user.Id.ToString()).SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        message = notificationMessage,
                        decisionId = decision.Id,
                        description = GetDecisionDescription(decision),
                        isRead = false,
                        sentAt = notification.SentAt, // UTC time
                        createdAt = notification.CreatedAt, // UTC time
                        updatedAt = notification.UpdatedAt
                    });
                }
            }
        }

        public async Task CreateAndSendTaskUpdateNotification(DecisionUpdateDto decision, IEnumerable<Guid> departmentIds)
        {
            var notificationMessage = $"Tasks are updated for your department.";

            // 1. Create a notification in the database for each department
            foreach (var departmentId in departmentIds)
            {
                // Get all users in the department
                var usersInDepartment = await _userManager.Users
                    .Where(u => u.DepartmentId == departmentId)
                    .ToListAsync();

                foreach (var user in usersInDepartment)
                {
                    // Create a unique notification record for each user
                    var notification = new Notification
                    {
                        Message = notificationMessage,
                        RecipientUserId = user.Id,
                        RecipientDepartmentId = departmentId,
                        RelatedEntityId = decision.Id,
                        RelatedEntityType = "Task",
                        IsRead = false,
                        SentAt = DateTime.UtcNow
                    };
                    _dbContext.Notifications.Add(notification);

                    // Send real-time notification to the individual user
                    await _hubContext.Clients.User(user.Id.ToString()).SendAsync("ReceiveNotification", new
                    {
                        message = notificationMessage,
                        decisionId = decision.Id,
                        description = decision.Description,
                        isRead = false,
                        sentAt = notification.SentAt,
                        id = notification.Id
                    });
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Notification>> GetPersistentNotificationsForUsersAsync(Guid userId)
        {
            try
            {
                var notifications = await _dbContext.Notifications
                    .Where(n => n.RecipientUserId == userId && !n.IsDeleted)
                    .OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt)
                    .ToListAsync();

                return notifications;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching persistent notifications: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Notification>> GetPersistentNotificationsAsync(Guid departmentId)
        {
            try
            {
                var notifications = await _dbContext.Notifications
                    .Where(n => n.RecipientDepartmentId == departmentId && !n.IsDeleted)
                    .OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt)
                    .ToListAsync();

                return notifications;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching persistent notifications: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(Guid departmentId)
        {
            try
            {
                var unreadCount = await _dbContext.Notifications
                    .Where(n => n.RecipientDepartmentId == departmentId && !n.IsRead && !n.IsDeleted)
                    .CountAsync();

                return unreadCount;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching unread notification count: {ex.Message}");
                throw;
            }
        }

        public async Task SetNotificationAsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _dbContext.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted);

                if (notification == null)
                {
                    throw new KeyNotFoundException("Notification not found.");
                }

                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow; // Store UTC time

                _dbContext.Notifications.Update(notification);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error setting notification as read: {ex.Message}");
                throw;
            }
        }

        // Helper method to get decision display name
        private string GetDecisionDisplayName(DecisionDto decision)
        {
            var type = decision.GetType();

            var titleProperty = type.GetProperty("Title");
            if (titleProperty != null)
            {
                var title = titleProperty.GetValue(decision)?.ToString();
                if (!string.IsNullOrEmpty(title))
                    return title;
            }

            var nameProperty = type.GetProperty("Name");
            if (nameProperty != null)
            {
                var name = nameProperty.GetValue(decision)?.ToString();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }

            var subjectProperty = type.GetProperty("Description");
            if (subjectProperty != null)
            {
                var subject = subjectProperty.GetValue(decision)?.ToString();
                if (!string.IsNullOrEmpty(subject))
                    return subject;
            }

            return $"Decision {decision.Id}";
        }

        // Helper method to get decision description
        private string GetDecisionDescription(DecisionDto decision)
        {
            var type = decision.GetType();

            var descriptionProperty = type.GetProperty("Description");
            if (descriptionProperty != null)
            {
                var description = descriptionProperty.GetValue(decision)?.ToString();
                if (!string.IsNullOrEmpty(description))
                    return description;
            }

            var detailsProperty = type.GetProperty("Details");
            if (detailsProperty != null)
            {
                var details = detailsProperty.GetValue(decision)?.ToString();
                if (!string.IsNullOrEmpty(details))
                    return details;
            }

            var contentProperty = type.GetProperty("Content");
            if (contentProperty != null)
            {
                var content = contentProperty.GetValue(decision)?.ToString();
                if (!string.IsNullOrEmpty(content))
                    return content;
            }

            return string.Empty;
        }

    }
}
