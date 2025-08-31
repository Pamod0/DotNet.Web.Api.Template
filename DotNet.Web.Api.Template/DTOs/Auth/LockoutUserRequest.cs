namespace ASP.NET_Core_Identity.DTOs.Auth
{
    public class LockoutUserRequest
    {
        public Guid UserId { get; set; }
        public TimeSpan? LockoutDuration { get; set; }
    }
}
