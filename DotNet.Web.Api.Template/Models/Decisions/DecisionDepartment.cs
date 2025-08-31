namespace DotNet.Web.Api.Template.Models.Decisions
{
    public class DecisionDepartment
    {
        public Guid DecisionId { get; set; }
        public Decision Decision { get; set; }

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
