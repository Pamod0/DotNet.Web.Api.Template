using ASP.NET_Core_Identity.Models.FileUploads;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP.NET_Core_Identity.Models.Auth
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        public Guid DepartmentId { get; set; }

        // Navigation property
        public virtual Department Department { get; set; } = null!;
        public Guid? ProfilePictureId { get; set; }
        public virtual SupportDocument? ProfilePicture { get; set; }

        // Helper property for full name
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public Guid CreatedUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid? UpdatedUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
        public Guid? DeletedUserId { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
