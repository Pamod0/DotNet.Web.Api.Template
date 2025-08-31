using ASP.NET_Core_Identity.Models.Decisions;
using System.ComponentModel.DataAnnotations;
using Task = ASP.NET_Core_Identity.Models.Decisions.Task;

namespace ASP.NET_Core_Identity.DTOs.Decision
{
    public class UpdateDecisionDto
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public string ReferenceId { get; set; } = string.Empty;

        public DateTime DecisionDate { get; set; }

        public DateTime Deadline { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DecisionType DecisionType { get; set; }

        public DecisionStatus Status { get; set; }

        public Guid MeetingId { get; set; }

        public ICollection<TaskDTO> Tasks { get; set; } = new List<TaskDTO>();
    }
}
