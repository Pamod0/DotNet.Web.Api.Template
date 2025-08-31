using DotNet.Web.Api.Template.DTOs;
using DotNet.Web.Api.Template.Repositories.Interfaces;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.Web.Api.Template.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileStorageController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileUploadRepository _fileUploadRepository;
        private readonly ILogger<FileStorageController> _logger;

        public FileStorageController(IFileStorageService fileStorageService, IFileUploadRepository fileUploadRepository, ILogger<FileStorageController> logger)
        {
            _fileStorageService = fileStorageService;
            _fileUploadRepository = fileUploadRepository;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a file to the specified folder
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="folderName">The folder name where the file will be stored</param>
        /// <returns>File upload result with file name and path</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string? relatedEntityId = null, [FromForm] string folderName = "General")
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

                var (originalFileName, relativePath) = await _fileStorageService.SaveFileAsync(file, folderName);

                _logger.LogInformation("File uploaded successfully: {FileName} to {FolderName}", originalFileName, folderName);

                if (!String.IsNullOrEmpty(relatedEntityId))
                {
                    var description = "Uploaded via API";

                    switch (folderName)
                    {
                        case "Decision":
                            description = "Support Document";
                            break;
                        case "Meeting":
                            description = "Support Document";
                            break;
                        default:
                            description = "Uploaded via API";
                            break;
                    }

                    _fileUploadRepository.SaveFileAsync(new SupportDocumentDto
                    {
                        FileName = originalFileName,
                        FilePath = relativePath,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        Description = description,
                    }, Guid.Parse(relatedEntityId), folderName).GetAwaiter().GetResult();
                }
                else
                {
                    _logger.LogWarning("Related entity ID is null or empty. File will not be associated with any entity.");
                }

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

        [AllowAnonymous]
        [HttpGet("download")]
        public IActionResult DownloadFile([FromQuery] string filePath)
        {
            // 1. Validate the file path to prevent directory traversal attacks
            if (string.IsNullOrWhiteSpace(filePath) || filePath.Contains(".."))
            {
                return BadRequest(new { error = "Invalid file path" });
            }

            // 2. Reconstruct the full physical path on the server
            var fullPath = Path.Combine(_fileStorageService.GetUploadsRootPath(), filePath);

            // 3. Check if the file exists
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { error = "File not found." });
            }

            // 4. Determine the content type
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            // 5. Return the file
            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType, Path.GetFileName(fullPath));
        }


        /// <summary>
        /// Uploads a file specifically to the meeting-minutes folder
        /// </summary>
        /// <param name="file">The meeting minutes file to upload</param>
        /// <returns>File upload result</returns>
        [HttpPost("upload/meeting-minutes")]
        public async Task<IActionResult> UploadMeetingMinutes(IFormFile file)
        {
            return await UploadFile(file, "meeting-minutes");
        }

        /// <summary>
        /// Deletes a file by its relative path
        /// </summary>
        /// <param name="filePath">The relative path of the file to delete</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("delete")]
        public IActionResult DeleteFile([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest(new { error = "File path is required" });
                }

                // Basic security check - ensure the path doesn't contain directory traversal attempts
                if (filePath.Contains("..") || filePath.Contains("\\..") || filePath.Contains("../"))
                {
                    return BadRequest(new { error = "Invalid file path" });
                }

                _fileStorageService.DeleteFile(filePath);

                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);

                return Ok(new
                {
                    success = true,
                    message = "File deleted successfully",
                    filePath = filePath,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting file: {FilePath}", filePath);
                return StatusCode(500, new { error = "An error occurred while deleting the file" });
            }
        }

        /// <summary>
        /// Gets file information (if you need to check if a file exists)
        /// </summary>
        /// <param name="filePath">The relative path of the file</param>
        /// <returns>File information</returns>
        [HttpGet("info")]
        public IActionResult GetFileInfo([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest(new { error = "File path is required" });
                }

                // Basic security check
                if (filePath.Contains(".."))
                {
                    return BadRequest(new { error = "Invalid file path" });
                }

                // You'll need to add this method to your service interface and implementation
                // For now, we'll just return basic info
                return Ok(new
                {
                    success = true,
                    filePath = filePath,
                    message = "Use this endpoint to get file information"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting file info: {FilePath}", filePath);
                return StatusCode(500, new { error = "An error occurred while retrieving file information" });
            }
        }

        [HttpGet("SupportDocuments")]
        public async Task<IActionResult> GetAllSupportDocuments([FromQuery] SupportDocumentTypesDto? supportDocumentTypesDto)
        {
            try
            {
                var documents = await _fileUploadRepository.GetAllSupportDocumentsAsync(supportDocumentTypesDto);

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







    //[ApiController]
    //[Route("api/[controller]")]
    //public class FileUploadController : ControllerBase
    //{
    //    private readonly IFileStorageService _fileStorageService;
    //    private readonly ILogger<FileUploadController> _logger;

    //    public FileUploadController(IFileStorageService fileStorageService, ILogger<FileUploadController> logger)
    //    {
    //        _fileStorageService = fileStorageService;
    //        _logger = logger;
    //    }

    //    [HttpPost("upload")]
    //    public async Task<IActionResult> UploadFile(IFormFile file, string folderName = "misc")
    //    {
    //        if (file == null || file.Length == 0)
    //        {
    //            return BadRequest("No file uploaded.");
    //        }
    //        try
    //        {
    //            var (fileName, filePath) = await _fileStorageService.SaveFileAsync(file, folderName);
    //            return Ok(new { fileName, filePath });
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error uploading file.");
    //            return StatusCode(500, "Internal server error");
    //        }
    //    }
    //}
}
