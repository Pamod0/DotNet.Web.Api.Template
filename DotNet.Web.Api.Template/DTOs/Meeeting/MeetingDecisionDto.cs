using DotNet.Web.Api.Template.Models.Decisions;

namespace DotNet.Web.Api.Template.DTOs.Meeeting
{
    public class MeetingDecisionDto
    {
        public Guid Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DecisionStatus Status { get; set; }
    }
}
