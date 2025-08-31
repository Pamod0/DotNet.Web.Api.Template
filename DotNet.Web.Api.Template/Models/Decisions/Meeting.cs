using DotNet.Web.Api.Template.Models.FileUploads;

namespace DotNet.Web.Api.Template.Models.Decisions
{
    public class Meeting : BaseEntity
    {
        //public new int Id { get; set; }
        public DateOnly MeetingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string Description { get; set; }
        public bool SendNotificationToParticipants { get; set; }

        // Navigation property (One meeting has many Decisions)  
        public ICollection<Decision> Decisions { get; set; } = new List<Decision>();

        // Navigation property for related Departments (Many-to-Many relationship)  
        public ICollection<MeetingDepartment> MeetingDepartments { get; set; } = new List<MeetingDepartment>();

        // Navigation property for the one SupportDocument 
        public virtual ICollection<SupportDocument> SupportDocuments { get; set; } = new List<SupportDocument>();
    }
}
