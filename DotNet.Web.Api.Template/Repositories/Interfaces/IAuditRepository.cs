using DotNet.Web.Api.Template.Models.Audit;

namespace DotNet.Web.Api.Template.Repositories.Interfaces
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
