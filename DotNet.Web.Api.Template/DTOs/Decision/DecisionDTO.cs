using DotNet.Web.Api.Template.DTOs.Department;
using DotNet.Web.Api.Template.Models.Decisions;
using System.ComponentModel.DataAnnotations;
using Task = DotNet.Web.Api.Template.Models.Decisions.Task;

namespace DotNet.Web.Api.Template.DTOs.Decision
{
    public class DecisionDto
    {
        public Guid Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public DateTime DecisionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public DecisionType DecisionType { get; set; }
        public DecisionStatus Status { get; set; }
        public Guid MeetingId { get; set; }
        public string? MeetingDescription { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public TaskCompletionDto? TaskCompletion { get; set; } = null;


        public ICollection<DepartmentReadDto> Departments { get; set; } = new List<DepartmentReadDto>();
        public ICollection<TaskReadDto> Tasks { get; set; } = new List<TaskReadDto>();
        public ICollection<SupportDocumentReadDto> SupportDocuments { get; set; } = new List<SupportDocumentReadDto>();
    }

    public class TaskCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public List<Guid> AssignedDepartmentIds { get; set; } = new List<Guid>();
        public bool isCompleted { get; set; }
    }

    public class TaskCompletionDto
    {
        public string? Progress { get; set; }
        public string? TaskCompletionText { get; set; }
    }

    public class SupportDocumentCreateDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string? Description { get; set; }
    }

    public class DecisionUpdateDto
    {
        [Required]
        public Guid Id { get; set; } // ID is required for updates

        [MaxLength(50)]
        public string? ReferenceId { get; set; } = string.Empty;

        public DateTime? DecisionDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; } = string.Empty;

        public DateTime? Deadline { get; set; }

        public DecisionType? DecisionType { get; set; }

        public DecisionStatus? Status { get; set; }

        public ICollection<Guid>? DepartmentsIds { get; set; } = new List<Guid>();

        public Guid? MeetingId { get; set; }

        public virtual ICollection<TaskCreateDto> Tasks { get; set; } = new List<TaskCreateDto>();
    }

    public class TaskReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
    }

    public class SupportDocumentReadDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string? Description { get; set; }
    }
}
