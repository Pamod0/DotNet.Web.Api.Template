using ASP.NET_Core_Identity.DTOs.User;
using ASP.NET_Core_Identity.Models.Auth;

namespace ASP.NET_Core_Identity.DTOs.Department
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
