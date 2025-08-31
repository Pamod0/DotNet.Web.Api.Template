using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.DTOs.User
{
    public class CreateUserDTO
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Role { get; set; }
    }
}
