using AutoMapper;
using DotNet.Web.Api.Template.Configurations;
using DotNet.Web.Api.Template.DTOs;
using DotNet.Web.Api.Template.Repositories.Interfaces;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DotNet.Web.Api.Template.Services
{
    public class LocalStorageService : IFileStorageService
    {
        private readonly string _uploadsRootPath;
        private readonly string _meetingMinutesFolderName; // You have this, but `folderName` parameter is used in SaveFileAsync
        private readonly IWebHostEnvironment _env; // Still useful if you have other static file needs or fallback logic
        private readonly IFileUploadRepository _fileUploadRepository;
        private readonly IMapper _mapper;

        public LocalStorageService(IOptions<FileStorageSettings> fileStorageSettings, IWebHostEnvironment env, IFileUploadRepository fileUploadRepository, IMapper mapper)
        {
            _uploadsRootPath = fileStorageSettings.Value.UploadsRootPath;
            _meetingMinutesFolderName = fileStorageSettings.Value.MeetingMinutesFolderName; // This can be used as a default folderName
            _env = env;
            _fileUploadRepository = fileUploadRepository;
            _mapper = mapper;

            // Ensure the root uploads directory exists
            if (!Directory.Exists(_uploadsRootPath))
            {
                Directory.CreateDirectory(_uploadsRootPath);
            }
        }

        public async Task<(string fileName, string filePath)> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.", nameof(file));
            }

            // Combine the configured root path with the specific folder for meeting minutes
            // The `folderName` parameter allows flexibility for other types of uploads
            var targetFolder = Path.Combine(_uploadsRootPath, folderName);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var fullFilePath = Path.Combine(targetFolder, uniqueFileName);

            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Store path relative to the configured _uploadsRootPath in the database
            // This is crucial for retrieving and deleting files later.
            var relativePath = Path.GetRelativePath(_uploadsRootPath, fullFilePath);
            return (file.FileName, relativePath.Replace("\\", "/")); // Normalize slashes for URLs/consistency
        }

        public void DeleteFile(string filePath) // <-- ADDED IMPLEMENTATION
        {
            // `filePath` here is the relative path stored in the database (e.g., "meeting-minutes/some-guid_original-name.pdf")
            // We need to reconstruct the full physical path on the server.
            var fullPathToDelete = Path.Combine(_uploadsRootPath, filePath);

            if (File.Exists(fullPathToDelete))
            {
                try
                {
                    File.Delete(fullPathToDelete);
                    // You might want to log this deletion for auditing or debugging
                    // Console.WriteLine($"File deleted successfully: {fullPathToDelete}");
                }
                catch (IOException ex)
                {
                    // Handle cases where the file might be locked or permissions are an issue
                    Console.Error.WriteLine($"Error deleting file '{fullPathToDelete}': {ex.Message}");
                    // Consider throwing a custom exception or logging with ILogger
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.Error.WriteLine($"Access denied when deleting file '{fullPathToDelete}': {ex.Message}");
                }
            }
            else
            {
                // Console.WriteLine($"Attempted to delete file, but it did not exist: {fullPathToDelete}");
            }
        }

        public string GetUploadsRootPath()
        {
            return _uploadsRootPath;
        }

        public async Task<IEnumerable<SupportDocumentDto>> GetAllSupportDocumentsAsync(SupportDocumentTypesDto? supportDocumentTypesDto)
        {
            var result = await _fileUploadRepository.GetAllSupportDocumentsAsync(supportDocumentTypesDto);

            var supportDocumentsDto = _mapper.Map<IEnumerable<SupportDocumentDto>>(result);

            return supportDocumentsDto;
        }

    }
}
