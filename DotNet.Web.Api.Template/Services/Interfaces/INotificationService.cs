using ASP.NET_Core_Identity.DTOs.Decision;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Models.Notification;

namespace ASP.NET_Core_Identity.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateAndSendMeetingNotification(MeetingDto meeting, IEnumerable<Guid> departmentIds);
        Task CreateAndSendDecisionNotification(DecisionDto decision, IEnumerable<Guid> departmentIds);
        Task CreateAndSendTaskNotification(DecisionDto decision, IEnumerable<Guid> departmentIds);
        Task CreateAndSendTaskUpdateNotification(DecisionUpdateDto decision, IEnumerable<Guid> departmentIds);
        Task<IEnumerable<Notification>> GetPersistentNotificationsForUsersAsync(Guid userId);
        Task<IEnumerable<Notification>> GetPersistentNotificationsAsync(Guid departmentId);
        Task<int> GetUnreadNotificationCountAsync(Guid departmentId);
        Task SetNotificationAsReadAsync(Guid notificationId);
    }
}
