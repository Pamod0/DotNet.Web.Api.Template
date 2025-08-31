using ASP.NET_Core_Identity.DTOs.Decision;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Decisions;
using ASP.NET_Core_Identity.Services;
using ASP.NET_Core_Identity.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Core_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DecisionsController : ControllerBase
    {
        private readonly IDecisionService _decisionService;
        private readonly ILogger<DecisionsController> _logger;
        private readonly INotificationService _notificationService;

        public DecisionsController(IDecisionService decisionService, ILogger<DecisionsController> logger, INotificationService notificationService)
        {
            _decisionService = decisionService;
            _logger = logger;
            _notificationService = notificationService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DecisionDto>> GetDecision(Guid id)
        {
            var decision = await _decisionService.GetDecisionByIdAsync(id);

            if (decision == null)
            {
                return NotFound();
            }

            return Ok(decision);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<PagedResponse<IEnumerable<DecisionDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _decisionService.GetAllDecisionsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving decisions.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetAll/Completed")]
        public async Task<ActionResult<PagedResponse<IEnumerable<DecisionDto>>>> GetAllCompleted([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _decisionService.GetAllCompletedDecisionsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving decisions.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<DecisionDto>> CreateDecision(AddDecisionDto addDecisionDto)
        {
            var createdDecision = await _decisionService.CreateDecisionAsync(addDecisionDto);

            if (addDecisionDto.SelectedDepartmentIds != null && addDecisionDto.SelectedDepartmentIds.Any())
            {
                await _notificationService.CreateAndSendDecisionNotification(createdDecision, addDecisionDto.SelectedDepartmentIds);
            }
            if (addDecisionDto.Tasks != null && addDecisionDto.Tasks.Any())
            {
                var taskDepartments = addDecisionDto.Tasks.SelectMany(t => t.AssignedDepartmentIds).Distinct();
                await _notificationService.CreateAndSendTaskNotification(createdDecision, taskDepartments);
            }

            return CreatedAtAction(nameof(GetDecision), new { id = createdDecision.Id }, createdDecision);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDecision(Guid id, DecisionUpdateDto decisionUpdateDto)
        {
            var result = await _decisionService.UpdateDecisionAsync(id, decisionUpdateDto);

            if (!result)
            {
                return NotFound();
            }

            return NoContent(); // 204 No Content for successful update
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchDecision(Guid id, [FromBody] JsonPatchDocument<UpdateDecisionDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var success = await _decisionService.UpdateDecisionPartialAsync(id, patchDoc);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDecision(Guid id)
        {
            var result = await _decisionService.DeleteDecisionAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent(); // 204 No Content for successful deletion
        }

        [HttpDelete("DeleteFile/{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            try
            {
                await _decisionService.DeleteFileAsync(fileId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"File with ID {fileId} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the file.");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
