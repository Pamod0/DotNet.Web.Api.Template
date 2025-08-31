using ASP.NET_Core_Identity.DTOs.Department;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Models;

namespace ASP.NET_Core_Identity.Services.Interfaces
{
    public interface IDepartmentService
    {
        //Task<PagedResponse<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync(PagedRequest request);
        Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id);
        Task<DepartmentWithAllDataDto?> GetDepartmentWithAllDataByIdAsync(Guid id);
        Task<PagedResponse<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync(PagedRequest request);
        Task<IEnumerable<DepartmentDropdownDto>> GetDepartmentDropdownAsync();
        Task<DepartmentDto> CreateDepartmentAsync(AddDepartmentDto addDepartmentDto);
        Task<bool> UpdateDepartmentAsync(UpdateDepartmentDto updateDepartmentDto);
        Task<bool> SoftDeleteDepartmentAsync(Guid id);
        Task<bool> DeleteDepartmentAsync(Guid id);
    }
}
