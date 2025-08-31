namespace DotNet.Web.Api.Template.Models.Notification
{
    public class Notification : BaseEntity
    {
        public string Message { get; set; }
        public Guid? RecipientUserId { get; set; } // Optional, for user-specific notifications
        public Guid? RecipientDepartmentId { get; set; } // Optional, for department-specific notifications
        public Guid? RelatedEntityId { get; set; } // The ID of the related item (e.g., meeting, decision)
        public string RelatedEntityType { get; set; } // The type of the related item (e.g., "Meeting", "Decision")
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}
