using ASP.NET_Core_Identity.Models.Auth;
using ASP.NET_Core_Identity.Models.Decisions;
using System.ComponentModel.DataAnnotations;
using Task = ASP.NET_Core_Identity.Models.Decisions.Task;


namespace ASP.NET_Core_Identity.Models
{
    public class Department : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ShortName { get; set; } = string.Empty;

        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<MeetingDepartment> MeetingDepartments { get; set; } = new List<MeetingDepartment>();
        public virtual ICollection<DecisionDepartment> DecisionDepartments { get; set; } = new List<DecisionDepartment>();
        public virtual ICollection<TaskDepartment> TaskDepartments { get; set; } = new List<TaskDepartment>();
    }
}
