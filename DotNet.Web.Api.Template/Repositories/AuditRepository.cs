using ASP.NET_Core_Identity.Data;
using ASP.NET_Core_Identity.DTOs.Department;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Audit;
using ASP.NET_Core_Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Core_Identity.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<AuditEntry?> GetAuditEntryByIdAsync(Guid id)
        {
            IQueryable<AuditEntry> query = _context.AuditEntries;
            return await query
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AuditEntry?> GetAuditEntryWithAllDataByIdAsync(Guid id)
        {
            IQueryable<AuditEntry> query = _context.AuditEntries;

            return await query
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<AuditEntry>> GetAllAuditEntriesAsync()
        {
            return await _context.AuditEntries
                           .ToListAsync();
        }

        public IQueryable<AuditEntry> GetAllAuditEntriesQueryable()
        {
            return _context.AuditEntries
                          .AsQueryable();
        }

        public IQueryable<UserActionLog> GetAllUserActionLogsQueryable()
        {
            return _context.UserActionLogs
                          .AsQueryable();
        }
    }
}
