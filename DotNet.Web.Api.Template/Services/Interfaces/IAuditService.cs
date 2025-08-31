using ASP.NET_Core_Identity.DTOs.Audit;
using ASP.NET_Core_Identity.Models;

namespace ASP.NET_Core_Identity.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogUserActionAsync(Guid userId, string action, string? details = null);
        Task<AuditEntryDto?> GetAuditEntryByIdAsync(Guid id);
        Task<PagedResponse<IEnumerable<AuditEntryDto>>> GetAllAuditEntriesAsync(PagedRequest request);
        Task<PagedResponse<IEnumerable<UserActionLogDto>>> GetAllUserActionLogsAsync(PagedRequest request);
    }
}
