using AutoMapper;
using DotNet.Web.Api.Template.Data;
using DotNet.Web.Api.Template.DTOs.Audit;
using DotNet.Web.Api.Template.Models;
using DotNet.Web.Api.Template.Models.Audit;
using DotNet.Web.Api.Template.Repositories.Interfaces;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace DotNet.Web.Api.Template.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditRepository _auditRepository;
        private readonly IMapper _mapper;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IAuditRepository auditRepository, IMapper mapper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _auditRepository = auditRepository;
            _mapper = mapper;
        }

        public async Task LogUserActionAsync(Guid userId, string action, string details = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userAction = new UserActionLog
            {
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                RequestPath = httpContext?.Request?.Path.ToString()
            };
            _context.UserActionLogs.Add(userAction);
            await _context.SaveChangesAsync();
        }

        public async Task<AuditEntryDto?> GetAuditEntryByIdAsync(Guid id)
        {
            var auditEntry = await _auditRepository.GetAuditEntryByIdAsync(id);

            if (auditEntry == null)
            {
                return null;
            }

            var auditEntryDto = _mapper.Map<AuditEntryDto>(auditEntry);

            return auditEntryDto;
        }

        public async Task<PagedResponse<IEnumerable<AuditEntryDto>>> GetAllAuditEntriesAsync(PagedRequest request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 10;

            var query = _auditRepository.GetAllAuditEntriesQueryable();

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                if (request.ExactMatch)
                {
                    query = query.Where(m => m.EntityName == request.SearchText);
                }
                else
                {
                    query = query.Where(m => m.EntityName.Contains(request.SearchText));
                }
            }

            query = query.OrderByDescending(m => m.Timestamp);

            var totalRecords = await query.CountAsync();

            var auditEntries = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var auditEntryDtos = auditEntries.Select(d => new AuditEntryDto
            {
                Id = d.Id,
                UserId = d.UserId,
                EntityName = d.EntityName,
                ActionType = d.ActionType,
                Timestamp = d.Timestamp,
                Changes = d.Changes,
                EntityId = d.EntityId,
                TotalAuditEntries = totalRecords
            });

            return new PagedResponse<IEnumerable<AuditEntryDto>>(request.Page, request.PageSize, totalRecords, auditEntryDtos);
        }

        public async Task<PagedResponse<IEnumerable<UserActionLogDto>>> GetAllUserActionLogsAsync(PagedRequest request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 10;

            var query = _auditRepository.GetAllUserActionLogsQueryable();

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                if (request.ExactMatch)
                {
                    query = query.Where(m => m.Action == request.SearchText);
                }
                else
                {
                    query = query.Where(m => m.Action.Contains(request.SearchText));
                }
            }

            query = query.OrderByDescending(m => m.Timestamp);

            var totalRecords = await query.CountAsync();

            var auditEntries = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var auditEntryDtos = auditEntries.Select(d => new UserActionLogDto
            {
                Id = d.Id,
                UserId = d.UserId,
                Action = d.Action,
                Timestamp = d.Timestamp,
                Details = d.Details,
                IpAddress = d.IpAddress,
                UserAgent = d.UserAgent,
                RequestPath = d.RequestPath
            });

            return new PagedResponse<IEnumerable<UserActionLogDto>>(request.Page, request.PageSize, totalRecords, auditEntryDtos);
        }
    }
}
