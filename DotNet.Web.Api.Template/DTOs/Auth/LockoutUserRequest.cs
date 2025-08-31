namespace DotNet.Web.Api.Template.DTOs.Auth
{
    public class LockoutUserRequest
    {
        public Guid UserId { get; set; }
        public TimeSpan? LockoutDuration { get; set; }
    }
}
