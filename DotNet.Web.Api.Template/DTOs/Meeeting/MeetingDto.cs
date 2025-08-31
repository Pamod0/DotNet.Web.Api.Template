using DotNet.Web.Api.Template.DTOs.Decision;
using DotNet.Web.Api.Template.DTOs.Department;
using DotNet.Web.Api.Template.Models.FileUploads;

namespace DotNet.Web.Api.Template.DTOs.Meeeting
{
    public class MeetingDto
    {
        public Guid Id { get; set; }
        public DateOnly MeetingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public ICollection<SupportDocumentDto>? SupportDocuments { get; set; }
        public string? MeetingMinutesFileName { get; set; } // To show the name of the uploaded file
        public string? MeetingMinutesFilePath { get; set; } // To provide a link to the file
        public List<DepartmentDto> Participants { get; set; } = new List<DepartmentDto>(); // Departments involved
        public bool SendNotificationToParticipants { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedUserId { get; set; }
        public int DecisionsCount { get; set; }
        public List<MeetingDecisionDto>? Decision { get; set; }
    }
}
