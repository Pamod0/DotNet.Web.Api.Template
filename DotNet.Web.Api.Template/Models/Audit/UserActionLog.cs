namespace DotNet.Web.Api.Template.Models.Audit
{
    public class UserActionLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } // e.g., "UserLoggedIn", "AttemptedLoginFailed", "GeneratedReportX"
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; } // Optional: More details about the action
        public string IpAddress { get; set; } // For security auditing
        public string UserAgent { get; set; } // For client info
        public string RequestPath { get; set; } // The API endpoint called
    }
}
