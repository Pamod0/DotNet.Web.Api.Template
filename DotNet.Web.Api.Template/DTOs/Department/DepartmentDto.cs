using DotNet.Web.Api.Template.DTOs.User;
using DotNet.Web.Api.Template.Models.Auth;

namespace DotNet.Web.Api.Template.DTOs.Department
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public int TotalDecisions { get; set; } = 0;
        public int CompletedDecisions { get; set; } = 0;
        public int PendingDecisions { get; set; } = 0;
    }

    public class DepartmentWithAllDataDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public int TotalDecisions { get; set; } = 0;
        public int CompletedDecisions { get; set; } = 0;
        public int PendingDecisions { get; set; } = 0;
        public int OverdueDecisions { get; set; } = 0;
        public ICollection<DepartmentUserDto>? DepartmentUsers { get; set; } = new List<DepartmentUserDto>();
    }
}
