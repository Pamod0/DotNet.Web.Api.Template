using DotNet.Web.Api.Template.Data;
using DotNet.Web.Api.Template.DTOs.Department;
using DotNet.Web.Api.Template.Models;
using DotNet.Web.Api.Template.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace DotNet.Web.Api.Template.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Department?> GetDepartmentByIdAsync(Guid id, bool includeRelated = false)
        {
            IQueryable<Department> query = _context.Departments;

            if (includeRelated)
            {
                query = query
                    .Where(m => !m.IsDeleted)
                    .Include(u => u.Users)
                    .Include(m => m.MeetingDepartments)
                        .ThenInclude(md => md.Meeting);
            }

            return await query
                .Where(m => !m.IsDeleted)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Department>> GetDepartmentsByIdsAsync(List<Guid> departmentIds)
        {
            return await _context.Departments
                          .Where(m => !m.IsDeleted && departmentIds.Contains(m.Id))
                          .Include(u => u.Users)
                          .Include(m => m.MeetingDepartments)
                              .ThenInclude(md => md.Meeting)
                          .ToListAsync();
        }

        public async Task<Department?> GetDepartmentWithAllDataByIdAsync(Guid id)
        {
            IQueryable<Department> query = _context.Departments;

            return await query
                .Where(m => !m.IsDeleted)
                .Include(u => u.Users)
                .Include(m => m.MeetingDepartments)
                    .ThenInclude(md => md.Meeting)
                .Include(d => d.DecisionDepartments)
                     .ThenInclude(d => d.Decision)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments
                          .Where(m => !m.IsDeleted)
                          .Include(u => u.Users)
                          .Include(m => m.MeetingDepartments)
                              .ThenInclude(md => md.Meeting)
                          .Include(d => d.DecisionDepartments)
                              .ThenInclude(d => d.Decision)
                           .ToListAsync();
        }

        public IQueryable<Department> GetAllDepartmentsQueryable()
        {
            return _context.Departments
                          .Where(m => !m.IsDeleted)
                          .Include(u => u.Users)
                          .Include(m => m.MeetingDepartments)
                              .ThenInclude(md => md.Meeting)
                          .AsQueryable();
        }

        public async Task<IEnumerable<DepartmentDropdownDto>> GetDepartmentDropdownAsync()
        {
            return await _context.Departments
                            .Where(d => !d.IsDeleted)
                            .OrderBy(d => d.Name)
                            .Select(d => new DepartmentDropdownDto
                            {
                                Id = d.Id,
                                Name = d.Name,
                                ShortName = d.ShortName
                            })
                            .ToListAsync();
        }

        public async Task<Department> AddDepartmentAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            _context.Entry(department).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteDepartmentAsync(Guid id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department != null)
            {
                department.IsDeleted = true;

                // Mark the entity as modified
                _context.Entry(department).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteDepartmentAsync(Guid id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DepartmentExists(Guid id)
        {
            return await _context.Departments.AnyAsync(e => e.Id == id);
        }
    }
}
