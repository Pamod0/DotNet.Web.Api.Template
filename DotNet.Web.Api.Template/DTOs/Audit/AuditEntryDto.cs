namespace DotNet.Web.Api.Template.DTOs.Audit
{
    public class AuditEntryDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string EntityName { get; set; }
        public string ActionType { get; set; } // e.g., "Created", "Updated", "Deleted"
        public DateTime Timestamp { get; set; }
        public string Changes { get; set; } // JSON string of old and new values
        public string EntityId { get; set; } // To link back to the modified entity
        public int? TotalAuditEntries { get; set; }
    }
}
