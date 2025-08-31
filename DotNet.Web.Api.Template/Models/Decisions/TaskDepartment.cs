namespace ASP.NET_Core_Identity.Models.Decisions
{
    public class TaskDepartment
    {
        public Guid TaskId { get; set; }
        public Task Task { get; set; } = null!;

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
    }
}
