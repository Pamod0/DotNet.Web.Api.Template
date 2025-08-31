using DotNet.Web.Api.Template.Data;
using DotNet.Web.Api.Template.Hubs;
using DotNet.Web.Api.Template.Models.Auth;
using DotNet.Web.Api.Template.Models.Notification;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DotNet.Web.Api.Template.Services
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

    }
}
