using ASP.NET_Core_Identity.DTOs.Department;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.DTOs.User;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Auth;
using ASP.NET_Core_Identity.Models.Decisions;
using ASP.NET_Core_Identity.Repositories;
using ASP.NET_Core_Identity.Repositories.Interfaces;
using ASP.NET_Core_Identity.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASP.NET_Core_Identity.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentService(IDepartmentRepository departmentRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;

        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id)
        {
            var department = await _departmentRepository.GetDepartmentByIdAsync(id, true);

            if (department == null)
            {
                return null;
            }

            var departmentDto = _mapper.Map<DepartmentDto>(department);

            return departmentDto;
        }

        public async Task<DepartmentWithAllDataDto?> GetDepartmentWithAllDataByIdAsync(Guid id)
        {
            var department = await _departmentRepository.GetDepartmentWithAllDataByIdAsync(id);

            if (department == null)
            {
                return null;
            }

            var departmentDto = _mapper.Map<DepartmentWithAllDataDto>(department);

            if (department.DecisionDepartments != null)
            {
                departmentDto.TotalDecisions = department.DecisionDepartments.Count();
                departmentDto.CompletedDecisions = department.DecisionDepartments.Count(dd => dd.Decision.Status == DecisionStatus.Completed);
                departmentDto.PendingDecisions = department.DecisionDepartments.Count(dd => dd.Decision.Status == DecisionStatus.Pending);
                departmentDto.OverdueDecisions = department.DecisionDepartments.Count(dd => dd.Decision.Status == DecisionStatus.Overdue);
            }

            if (department.Users != null)
            {
                departmentDto.DepartmentUsers = _mapper.Map<ICollection<DepartmentUserDto>>(department.Users);

                foreach (var user in department.Users)
                {
                    var usrRole = await _userManager.GetRolesAsync(user);

                    foreach (var departmentUsers in departmentDto.DepartmentUsers)
                    {
                        if (departmentUsers.Id == user.Id)
                        {
                            departmentUsers.Role = usrRole.FirstOrDefault() ?? string.Empty;
                        }

                    }
                }
            }
            else
            {
                departmentDto.DepartmentUsers = new List<DepartmentUserDto>();
            }

            return departmentDto;
        }

        public async Task<PagedResponse<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync(PagedRequest request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 10;

            var query = _departmentRepository.GetAllDepartmentsQueryable();

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                if (request.ExactMatch)
                {
                    query = query.Where(m => m.Name == request.SearchText);
                }
                else
                {
                    query = query.Where(m => m.Name.Contains(request.SearchText));
                }
            }

            query = query.OrderByDescending(m => m.Name);

            var totalRecords = await query.CountAsync();

            // The key change is here. Include the necessary navigation properties.
            var departments = await query
                .Include(d => d.DecisionDepartments)
                    .ThenInclude(dd => dd.Decision)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var departmentDtos = departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                ShortName = d.ShortName,
                // Calculate the decision counts based on the DecisionStatus enum
                TotalDecisions = d.DecisionDepartments.Count(),
                CompletedDecisions = d.DecisionDepartments.Count(dd => dd.Decision.Status == DecisionStatus.Completed),
                PendingDecisions = d.DecisionDepartments.Count(dd => dd.Decision.Status == DecisionStatus.Pending)
            });

            return new PagedResponse<IEnumerable<DepartmentDto>>(request.Page, request.PageSize, totalRecords, departmentDtos);
        }

        public async Task<IEnumerable<DepartmentDropdownDto>> GetDepartmentDropdownAsync()
        {
            var departmentDropdown = await _departmentRepository.GetDepartmentDropdownAsync();

            if (departmentDropdown == null || !departmentDropdown.Any())
            {
                return new List<DepartmentDropdownDto>();
            }

            return departmentDropdown;
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(AddDepartmentDto addDepartmentDto)
        {
            var department = _mapper.Map<Department>(addDepartmentDto);

            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                throw new InvalidOperationException("User ID is required to create a meeting.");
            }

            if (!Guid.TryParse(userIdString, out var userId))
            {
                throw new InvalidOperationException("Invalid User ID format.");
            }

            var createdDepartment = await _departmentRepository.AddDepartmentAsync(department);

            var departmentDto = _mapper.Map<DepartmentDto>(createdDepartment);

            return departmentDto;
        }

        public async Task<bool> UpdateDepartmentAsync(UpdateDepartmentDto updateDepartmentDto)
        {
            var existingDepartment = await _departmentRepository.GetDepartmentByIdAsync(updateDepartmentDto.Id, includeRelated: true);

            if (existingDepartment == null)
            {
                return false;
            }

            if (updateDepartmentDto.Name != null)
            {
                existingDepartment.Name = updateDepartmentDto.Name;
            }

            if (updateDepartmentDto.ShortName != null)
            {
                existingDepartment.ShortName = updateDepartmentDto.ShortName;
            }

            try
            {
                await _departmentRepository.UpdateDepartmentAsync(existingDepartment);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error updating departmen: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> SoftDeleteDepartmentAsync(Guid id)
        {
            var departmentToDelete = await _departmentRepository.GetDepartmentByIdAsync(id, includeRelated: true);
            if (departmentToDelete == null)
            {
                return false;
            }

            departmentToDelete.IsDeleted = true;
            try
            {
                await _departmentRepository.UpdateDepartmentAsync(departmentToDelete);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error soft deleting department: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> DeleteDepartmentAsync(Guid id)
        {
            var departmentToDelete = await _departmentRepository.GetDepartmentByIdAsync(id, includeRelated: true);

            if (departmentToDelete == null)
            {
                return false;
            }

            await _departmentRepository.DeleteDepartmentAsync(departmentToDelete.Id);

            return true;
        }

    }
}
