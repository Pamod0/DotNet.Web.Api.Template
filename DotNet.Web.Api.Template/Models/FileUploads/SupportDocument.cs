using DotNet.Web.Api.Template.Models.Auth;
using DotNet.Web.Api.Template.Models.Decisions;
using System.ComponentModel.DataAnnotations;
using Task = DotNet.Web.Api.Template.Models.Decisions.Task;

namespace DotNet.Web.Api.Template.Models.FileUploads
{
    public class SupportDocument : BaseEntity
    {

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        //public DateTime UploadDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Foreign Key
        public Guid? DecisionId { get; set; }

        // Navigation property
        public virtual Decision? Decision { get; set; } = null!;

        // Foreign key to Task
        public Guid? TaskId { get; set; }
        // Navigation property
        public virtual Task? Task { get; set; } = null!;

        // Foreign key to Meeting (nullable for one-to-zero-or-one)
        public Guid? MeetingId { get; set; }
        // Navigation property to Meeting
        public virtual Meeting? Meeting { get; set; }
        // Foreign key to ApplicationUser (UserProfile)
        public Guid UserProfileID { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
