using DotNet.Web.Api.Template.DTOs.Department;
using DotNet.Web.Api.Template.DTOs.Meeeting;
using DotNet.Web.Api.Template.Models;

namespace DotNet.Web.Api.Template.Services.Interfaces
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
