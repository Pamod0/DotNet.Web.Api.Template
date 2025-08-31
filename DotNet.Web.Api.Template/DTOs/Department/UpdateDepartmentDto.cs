using System.ComponentModel.DataAnnotations;

namespace DotNet.Web.Api.Template.DTOs.Department
{
    public class UpdateDepartmentDto
    {
        [Required]
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(10)]
        public string? ShortName { get; set; }
    }
}
