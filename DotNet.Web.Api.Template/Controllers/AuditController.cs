using DotNet.Web.Api.Template.DTOs.Audit;
using DotNet.Web.Api.Template.Models;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.Web.Api.Template.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet("AuditEntry/GetAll")]
        public async Task<ActionResult<PagedResponse<IEnumerable<AuditEntryDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _auditService.GetAllAuditEntriesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving audit entries.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("UserAction/GetAll")]
        public async Task<ActionResult<PagedResponse<IEnumerable<UserActionLogDto>>>> GetAllUserActions([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _auditService.GetAllUserActionLogsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving audit entries.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
