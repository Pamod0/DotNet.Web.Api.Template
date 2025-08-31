using ASP.NET_Core_Identity.DTOs.Department;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Decisions;
using Task = System.Threading.Tasks.Task;

namespace ASP.NET_Core_Identity.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetDepartmentByIdAsync(Guid id, bool includeRelated = false);
        Task<IEnumerable<Department>> GetDepartmentsByIdsAsync(List<Guid> departmentIds);
        Task<Department?> GetDepartmentWithAllDataByIdAsync(Guid id);
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        IQueryable<Department> GetAllDepartmentsQueryable();
        Task<IEnumerable<DepartmentDropdownDto>> GetDepartmentDropdownAsync();
        Task<Department> AddDepartmentAsync(Department meeting);
        Task UpdateDepartmentAsync(Department meeting);
        Task DeleteDepartmentAsync(Guid id);
        Task<bool> DepartmentExists(Guid id);
    }
}
