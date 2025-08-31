using DotNet.Web.Api.Template.Models.Decisions;
using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.DTOs.Decision
{
    public class CreateDecisionDTO
    {
        [Required]
        [MaxLength(50)]
        public string ReferenceId { get; set; } = string.Empty;

        [Required]
        public DateTime DecisionDate { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime? Deadline { get; set; }

        [Required]
        public DecisionType DecisionType { get; set; }

        [Required]
        public DecisionStatus Status { get; set; }

        public List<CreateTaskDTO> Tasks { get; set; } = new List<CreateTaskDTO>();
    }
}
