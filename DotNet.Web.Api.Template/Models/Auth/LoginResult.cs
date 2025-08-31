namespace DotNet.Web.Api.Template.Models.Auth
{
    public class LoginResult : ApiResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public Guid? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string>? Roles { get; set; }
        public string? UserDepartment { get; set; }
        public Guid UserDepartmentId { get; set; }
        public bool? RequiresTwoFactor { get; set; }
        public IList<string>? Providers { get; set; }
    }
}
