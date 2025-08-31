using DotNet.Web.Api.Template.Models.Auth;
using System.ComponentModel.DataAnnotations;

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

        public Guid UserProfileID { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
