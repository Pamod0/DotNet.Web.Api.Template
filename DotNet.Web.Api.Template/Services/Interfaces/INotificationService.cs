using DotNet.Web.Api.Template.DTOs.Decision;
using DotNet.Web.Api.Template.DTOs.Meeeting;
using DotNet.Web.Api.Template.Models.Notification;

namespace DotNet.Web.Api.Template.Services.Interfaces
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
