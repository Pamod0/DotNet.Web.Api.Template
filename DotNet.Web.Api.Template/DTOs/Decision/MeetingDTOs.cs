using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.DTOs.Decision
{
    public class MeetingCreateDto
    {
        [Required]
        public DateOnly MeetingDate { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly? EndTime { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        [MaxLength(500)]

        //public string? MeetingMinutesUrl { get; set; }
        public bool SendNotificationToParticipants { get; set; }
        [Required]
        public List<int> DepartmentIds { get; set; } = new List<int>();
    }

    public class MeetingUpdateDto
    {
        [Required]
        public Guid Id { get; set; } // ID is required for updates
        [Required]
        public DateOnly MeetingDate { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly EndTime { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? MeetingMinutesUrl { get; set; }
        public bool SendNotificationToParticipants { get; set; }
        [Required]
        public List<int> DepartmentIds { get; set; } = new List<int>(); // For assigning departments
    }

    public class MeetingReadDto : BaseDTO
    {
        public DateOnly MeetingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? MeetingMinutesUrl { get; set; }
        public bool SendNotificationToParticipants { get; set; }
        public ICollection<DepartmentReadDto> Departments { get; set; } = new List<DepartmentReadDto>();
    }

    public class DepartmentReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
    }
}
