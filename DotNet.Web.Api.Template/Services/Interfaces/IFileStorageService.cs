namespace ASP.NET_Core_Identity.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<(string fileName, string filePath)> SaveFileAsync(IFormFile file, string folderName);
        void DeleteFile(string filePath);
        string GetUploadsRootPath();
    }
}
