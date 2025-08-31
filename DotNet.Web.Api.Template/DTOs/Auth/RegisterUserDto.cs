using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.DTOs.Auth
{
    public class RegisterUserDTO
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
    }
}
