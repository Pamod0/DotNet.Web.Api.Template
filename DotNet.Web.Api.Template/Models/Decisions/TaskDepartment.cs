namespace DotNet.Web.Api.Template.Models.Decisions
{
    public class TaskDepartment
    {
        public Guid TaskId { get; set; }
        public Task Task { get; set; } = null!;

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
    }
}
