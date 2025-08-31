using ASP.NET_Core_Identity.DTOs;
using ASP.NET_Core_Identity.DTOs.User;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Auth;
using ASP.NET_Core_Identity.Repositories.Interfaces;
using ASP.NET_Core_Identity.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP.NET_Core_Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly ILogger<UserManagementController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileUploadRepository _fileUploadRepository;
        private readonly IUserRepository _userRepository;

        public UserManagementController(
            ILogger<UserManagementController> logger,
            UserManager<ApplicationUser> userManager,
            IUserService userService,
            IFileStorageService fileStorageService,
            IFileUploadRepository fileUploadRepository,
            IUserRepository userRepository
            )
        {
            _logger = logger;
            _userManager = userManager;
            _userService = userService;
            _fileStorageService = fileStorageService;
            _fileUploadRepository = fileUploadRepository;
            _userRepository = userRepository;
        }


        [HttpGet("GetUserById/{userId}")]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user.");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Errors = new[] { ex.Message }
                });
            }
        }

        [HttpGet("UserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.EmailConfirmed,
                Roles = roles
            });
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll([FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _userService.GetAllUsersAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("CreateUser")]
        [Consumes("multipart/form-data")] // Required for file uploads
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> CreateUserWithRole([FromForm] CreateUserDTO createUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var result = await _userService.CreateUserWithRoleAsync(createUser);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin user creation");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred during admin user creation.",
                    Errors = new[] { ex.Message }
                });
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<ActionResult<ApiResponse>> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var result = await _userService.UpdateUserAsync(updateUserDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user.");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred while updating the user.",
                    Errors = new[] { ex.Message }
                });
            }
        }

        [HttpGet("GetAllUserRoles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _userService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving roles.");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpDelete("DeleteUser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> DeleteUser(Guid userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the user.");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred while deleting the user.",
                    Errors = new[] { ex.Message }
                });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string relatedEntityId, [FromForm] string folderName = "General")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file provided or file is empty" });
                }

                // Optional: Add file size validation
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { error = "File size exceeds maximum allowed size (10MB)" });
                }

                // Optional: Add file type validation
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { error = "File type not allowed" });
                }

                var supportDocumentId = await _userRepository.GetFirstProfilePicIdAsync(Guid.Parse(relatedEntityId));

                if (supportDocumentId != null)
                {

                    if (supportDocumentId.HasValue)
                    {
                        string? filePath = await _fileUploadRepository.GetFilePathByIdAsync(supportDocumentId.Value);
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            _fileStorageService.DeleteFile(filePath);
                        }
                        else
                        {
                            _logger.LogWarning("File path is null or empty for support document ID: {SupportDocumentId}", supportDocumentId.Value);
                        }

                        await _fileUploadRepository.DeleteFileAsync(supportDocumentId.Value);
                    }
                    else
                    {
                        _logger.LogWarning("Support document ID is null. Skipping file path retrieval.");
                        return BadRequest(new { error = "Invalid related entity ID or no associated file found." });
                    }

                }


                var (originalFileName, relativePath) = await _fileStorageService.SaveFileAsync(file, folderName);
                _logger.LogInformation("File uploaded successfully: {FileName} to {FolderName}", originalFileName, folderName);

                _fileUploadRepository.SaveFileAsync(new SupportDocumentDto
                {
                    FileName = originalFileName,
                    FilePath = relativePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    Description = "Uploaded via API"
                }, Guid.Parse(relatedEntityId), folderName).GetAwaiter().GetResult();

                return Ok(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    data = new
                    {
                        originalFileName = originalFileName,
                        filePath = relativePath,
                        folderName = folderName,
                        fileSize = file.Length,
                        uploadedAt = DateTime.UtcNow
                    }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid file upload attempt: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file");
                return StatusCode(500, new { error = "An error occurred while uploading the file" });
            }
        }


        [HttpGet("SupportDocuments")]
        public async Task<IActionResult> GetAllSupportDocuments([FromQuery] SupportDocumentTypesDto? supportDocumentTypesDto)
        {
            try
            {
                var documents = await _userRepository.GetAllSupportDocumentsAsync(supportDocumentTypesDto);

                return Ok(new
                {
                    success = true,
                    data = documents
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving support documents");
                return StatusCode(500, new { error = "An error occurred while retrieving support documents" });
            }
        }

    }
}
