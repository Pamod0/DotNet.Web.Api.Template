using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.DTOs.User
{
    public class UpdateUserDto
    {
        [Required]
        public required Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? UserAccountStatus { get; set; } = null;
    }
}
