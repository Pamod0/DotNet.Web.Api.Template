using DotNet.Web.Api.Template.DTOs.Department;
using DotNet.Web.Api.Template.DTOs.Meeeting;
using DotNet.Web.Api.Template.Models;
using DotNet.Web.Api.Template.Services;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DotNet.Web.Api.Template.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartmentById(Guid id)
        {
            var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);
            if (departmentDto == null)
            {
                return NotFound($"Department with ID {id} not found.");
            }
            return Ok(departmentDto);
        }

        [HttpGet("WithAllData/{id}")]
        public async Task<ActionResult<DepartmentWithAllDataDto>> GetDepartmentWithAllDataById(Guid id)
        {
            try
            {
                var departmentWithAllData = await _departmentService.GetDepartmentWithAllDataByIdAsync(id);
                if (departmentWithAllData == null)
                {
                    return NotFound($"Department with ID {id} not found.");
                }
                return Ok(departmentWithAllData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving department with all data.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<PagedResponse<IEnumerable<DepartmentDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _departmentService.GetAllDepartmentsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving departments.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Dropdown")]
        public async Task<ActionResult<IEnumerable<DepartmentDropdownDto>>> GetDepartmentsForDropdown()
        {
            try
            {
                var departments = await _departmentService.GetDepartmentDropdownAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving departments for dropdown.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")] // Required for file uploads
        public async Task<IActionResult> AddDepartment([FromForm] AddDepartmentDto addDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var departmentDto = await _departmentService.CreateDepartmentAsync(addDepartmentDto);
                return Ok(departmentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the department: {ex.Message}");
            }
        }

        [HttpPatch("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateDepartment([FromRoute] Guid id, [FromForm] UpdateDepartmentDto updateDepartmentDto)
        {
            if (id != updateDepartmentDto.Id)
            {
                return BadRequest("Department ID in route does not match body.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            Guid modifiedByUserId = Guid.Parse(userId);

            var updated = await _departmentService.UpdateDepartmentAsync(updateDepartmentDto);

            if (!updated)
            {
                var existingDepartment = await _departmentService.GetDepartmentByIdAsync(id);
                if (existingDepartment == null)
                {
                    return NotFound($"Department with ID {id} not found.");
                }
                return StatusCode(500, "An error occurred while updating the department.");
            }

            var updatedDepartmentDto = await _departmentService.GetDepartmentByIdAsync(id);
            if (updatedDepartmentDto == null)
            {
                return StatusCode(500, "Department updated but could not retrieve latest data.");
            }

            return Ok(updatedDepartmentDto);
        }

        [HttpDelete("SoftDelete/{id}")]
        public async Task<IActionResult> SoftDeleteMeeting(Guid id)
        {
            var result = await _departmentService.SoftDeleteDepartmentAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent(); // 204 No Content for successful deletion
        }

        [HttpDelete("HardDelete/{id}")]
        public async Task<IActionResult> DeleteMeeting(Guid id)
        {
            var result = await _departmentService.DeleteDepartmentAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent(); // 204 No Content for successful deletion
        }

    }
}
