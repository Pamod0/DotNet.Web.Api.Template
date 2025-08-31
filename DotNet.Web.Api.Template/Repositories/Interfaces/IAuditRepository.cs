using ASP.NET_Core_Identity.DTOs.Department;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Audit;

namespace ASP.NET_Core_Identity.Repositories.Interfaces
{
    public interface IAuditRepository
    {
        Task<AuditEntry?> GetAuditEntryByIdAsync(Guid id);
        //Task<AuditEntry?> GetAuditEntryWithAllDataByIdAsync(Guid id);
        Task<IEnumerable<AuditEntry>> GetAllAuditEntriesAsync();
        IQueryable<AuditEntry> GetAllAuditEntriesQueryable();
        IQueryable<UserActionLog> GetAllUserActionLogsQueryable();
        //Task<IEnumerable<AuditEntryDropdownDto>> GetAuditEntryDropdownAsync();
    }
}
