using ASP.NET_Core_Identity.Models.FileUploads;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP.NET_Core_Identity.Models.Decisions
{
    public class Decision : BaseEntity
    {

        [Required]
        [MaxLength(50)]
        public string ReferenceId { get; set; } = string.Empty;

        [Required]
        public DateTime DecisionDate { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime Deadline { get; set; }

        [Required]
        public DecisionType DecisionType { get; set; } = DecisionType.Ongoing;

        [Required]
        public DecisionStatus Status { get; set; } = DecisionStatus.Pending;

        // Foreign key
        public Guid MeetingId { get; set; }
        // Navigation property (Each Decison belongs to one Meeting)
        public virtual Meeting Meeting { get; set; } = null!;

        // Navigation properties for relationships
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

        public virtual ICollection<SupportDocument> SupportDocuments { get; set; } = new List<SupportDocument>();

        // NEW: Navigation property for related Departments directly associated with this Decision
        public virtual ICollection<DecisionDepartment> DecisionDepartments { get; set; } = new List<DecisionDepartment>();

        // Helper property to get departments through tasks (not mapped to database)
        //[NotMapped]
        //public IEnumerable<Department> Departments => Tasks.Select(t => t.Department).Distinct();
    }

    public enum DecisionType
    {
        Ongoing,
        Completed
    }

    public enum DecisionStatus
    {
        Pending,
        InProgress,
        Completed,
        Overdue
    }
}
