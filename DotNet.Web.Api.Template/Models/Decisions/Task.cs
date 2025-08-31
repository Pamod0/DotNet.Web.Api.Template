using ASP.NET_Core_Identity.Models.FileUploads;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Core_Identity.Models.Decisions
{
    public class Task : BaseEntity
    {

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public bool isCompleted { get; set; } = false;

        // Foreign Keys
        public Guid DecisionId { get; set; }

        // Navigation properties
        public virtual Decision Decision { get; set; } = null!;

        public virtual ICollection<SupportDocument> SupportDocuments { get; set; } = new List<SupportDocument>();

        public virtual ICollection<TaskDepartment> TaskDepartments { get; set; } = new List<TaskDepartment>();
    }
}
