using DotNet.Web.Api.Template.Models.FileUploads;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNet.Web.Api.Template.Models.Auth
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

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
