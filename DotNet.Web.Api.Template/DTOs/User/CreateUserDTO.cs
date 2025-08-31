using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Core_Identity.DTOs.User
{
    public class CreateUserDTO
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        public required Guid DepartmentId { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Role { get; set; }
    }
}
