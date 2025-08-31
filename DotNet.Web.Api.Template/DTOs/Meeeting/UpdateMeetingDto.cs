using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASP.NET_Core_Identity.DTOs.Meeeting
{
    public class UpdateMeetingDto
    {
        [Required]
        public Guid Id { get; set; }
        public DateOnly? MeetingDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string? Description { get; set; }
        public IFormFile? MeetingMinutesFile { get; set; }
        public List<Guid>? DepartmentIds { get; set; }
        public bool? SendNotificationToParticipants { get; set; }
    }
}
