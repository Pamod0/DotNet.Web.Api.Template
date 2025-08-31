using ASP.NET_Core_Identity.Models.Decisions;

namespace ASP.NET_Core_Identity.DTOs.Meeeting
{
    public class MeetingDecisionDto
    {
        public Guid Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DecisionStatus Status { get; set; }
    }
}
