namespace ASP.NET_Core_Identity.DTOs.Decision
{
    public class TaskDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public Guid DepartmentId { get; set; }
        public bool IsCompleted { get; set; }
    }
}
