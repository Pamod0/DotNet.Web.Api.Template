using DotNet.Web.Api.Template.DTOs.Audit;
using DotNet.Web.Api.Template.Models;

namespace DotNet.Web.Api.Template.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogUserActionAsync(Guid userId, string action, string? details = null);
        Task<AuditEntryDto?> GetAuditEntryByIdAsync(Guid id);
        Task<PagedResponse<IEnumerable<AuditEntryDto>>> GetAllAuditEntriesAsync(PagedRequest request);
        Task<PagedResponse<IEnumerable<UserActionLogDto>>> GetAllUserActionLogsAsync(PagedRequest request);
    }
}
