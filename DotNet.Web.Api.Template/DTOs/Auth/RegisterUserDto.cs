using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Core_Identity.DTOs.Auth
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

        public string? Department { get; set; }
    }
}
