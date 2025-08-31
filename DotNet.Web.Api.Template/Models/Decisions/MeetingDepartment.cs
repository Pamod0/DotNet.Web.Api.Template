namespace DotNet.Web.Api.Template.Models.Decisions
{
    public class MeetingDepartment
    {
        public Guid MeetingId { get; set; }
        public Meeting Meeting { get; set; }
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
