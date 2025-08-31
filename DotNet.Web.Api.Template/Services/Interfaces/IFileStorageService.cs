namespace DotNet.Web.Api.Template.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<(string fileName, string filePath)> SaveFileAsync(IFormFile file, string folderName);
        void DeleteFile(string filePath);
        string GetUploadsRootPath();
    }
}
