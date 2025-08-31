using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Core_Identity.DTOs.Decision
{
    public class CreateTaskDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }
    }
}
