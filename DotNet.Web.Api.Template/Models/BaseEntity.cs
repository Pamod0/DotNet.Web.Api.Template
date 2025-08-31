namespace ASP.NET_Core_Identity.Models
{
    public class BaseEntity
    {
        public Guid Id { get; set; }

        public Guid CreatedUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid? UpdatedUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
        public Guid? DeletedUserId { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}