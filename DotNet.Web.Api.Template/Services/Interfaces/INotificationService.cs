using DotNet.Web.Api.Template.Models.Notification;

namespace DotNet.Web.Api.Template.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetPersistentNotificationsForUsersAsync(Guid userId);
        Task<IEnumerable<Notification>> GetPersistentNotificationsAsync(Guid departmentId);
        Task<int> GetUnreadNotificationCountAsync(Guid departmentId);
        Task SetNotificationAsReadAsync(Guid notificationId);
    }
}
