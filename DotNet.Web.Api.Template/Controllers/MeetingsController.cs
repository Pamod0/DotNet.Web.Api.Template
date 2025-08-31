using DotNet.Web.Api.Template.DTOs.Decision;
using DotNet.Web.Api.Template.DTOs.Meeeting;
using DotNet.Web.Api.Template.Hubs;
using DotNet.Web.Api.Template.Models;
using DotNet.Web.Api.Template.Services;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DotNet.Web.Api.Template.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly ILogger<MeetingsController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public MeetingsController(
            IMeetingService meetingService,
            ILogger<MeetingsController> logger,
            IHubContext<NotificationHub> hubContext,
            INotificationService notificationService)
        {
            _meetingService = meetingService;
            _logger = logger;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeetingById(Guid id)
        {
            var meetingDto = await _meetingService.GetMeetingByIdAsync(id);
            if (meetingDto == null)
            {
                return NotFound($"Meeting with ID {id} not found.");
            }
            return Ok(meetingDto);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<PagedResponse<IEnumerable<MeetingReadDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _meetingService.GetAllMeetingsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving meetings.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Dropdown")]
        public async Task<ActionResult<IEnumerable<MeetingDropdownDto>>> GetMeetingsDropdown()
        {
            try
            {
                var meetings = await _meetingService.GetMeetingDropdownAsync();
                return Ok(meetings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving meetings for dropdown.");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost]
        [Consumes("multipart/form-data")] // Required for file uploads
        public async Task<IActionResult> AddMeeting([FromForm] AddMeetingDto addMeetingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var meetingDto = await _meetingService.CreateMeetingAsync(addMeetingDto);

                // Send notification
                if (addMeetingDto.DepartmentIds != null && addMeetingDto.DepartmentIds.Any() && addMeetingDto.SendNotificationToParticipants)
                {
                    await _notificationService.CreateAndSendMeetingNotification(meetingDto, addMeetingDto.DepartmentIds);
                }

                return Ok(meetingDto);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework like Serilog, NLog)
                return StatusCode(500, $"An error occurred while adding the meeting: {ex.Message}");
            }
        }

        [HttpPatch("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMeeting([FromRoute] Guid id, [FromForm] UpdateMeetingDto updateMeetingDto)
        {
            if (id != updateMeetingDto.Id)
            {
                return BadRequest("Meeting ID in route does not match body.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            Guid modifiedByUserId = Guid.Parse(userId);

            var updated = await _meetingService.UpdateMeetingAsync(updateMeetingDto);

            if (!updated)
            {
                var existingMeeting = await _meetingService.GetMeetingByIdAsync(id);
                if (existingMeeting == null)
                {
                    return NotFound($"Meeting with ID {id} not found.");
                }
                return StatusCode(500, "An error occurred while updating the meeting.");
            }

            var updatedMeetingDto = await _meetingService.GetMeetingByIdAsync(id);
            if (updatedMeetingDto == null)
            {
                return StatusCode(500, "Meeting updated but could not retrieve latest data.");
            }

            return Ok(updatedMeetingDto);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(Guid id)
        {
            var result = await _meetingService.DeleteMeetingAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent(); // 204 No Content for successful deletion
        }

        [HttpGet("Latest")]
        public async Task<IActionResult> GetLatestMeeting()
        {
            try
            {
                var meetingDto = await _meetingService.GetLatestMeetingAsync();
                if (meetingDto == null)
                {
                    return NotFound("No meetings found.");
                }
                return Ok(meetingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the latest meeting.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("DeleteFile/{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            try
            {
                await _meetingService.DeleteFileAsync(fileId);
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
