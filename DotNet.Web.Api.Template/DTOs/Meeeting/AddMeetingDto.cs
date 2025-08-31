using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Core_Identity.DTOs.Meeeting
{
    public class AddMeetingDto
    {
        [Required(ErrorMessage = "Meeting Date is required.")]
        public DateOnly MeetingDate { get; set; }

        [Required(ErrorMessage = "Meeting Time is required.")]
        public TimeOnly StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        public List<Guid>? DepartmentIds { get; set; }

        public IFormFile? MeetingMinutesFile { get; set; }

        public bool SendNotificationToParticipants { get; set; } = true;
    }
}
