using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.DTOs.Decision
{
    public class AddDecisionDto
    {
        [Required]
        [MaxLength(50)]
        public string ReferenceId { get; set; } = string.Empty;

        [Required]
        public DateTime DecisionDate { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Deadline { get; set; }

        public Guid MeetingId { get; set; }

        public List<Guid> SelectedDepartmentIds { get; set; } = new List<Guid>();

        public List<TaskCreateDto> Tasks { get; set; } = new();

    }
}
