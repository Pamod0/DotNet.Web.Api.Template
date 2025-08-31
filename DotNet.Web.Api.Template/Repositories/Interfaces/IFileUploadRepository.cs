using DotNet.Web.Api.Template.DTOs;
using DotNet.Web.Api.Template.Models.FileUploads;

namespace DotNet.Web.Api.Template.Repositories.Interfaces
{
    public interface IFileUploadRepository
    {
        Task SaveFileAsync(SupportDocumentDto supportDocumentDto, Guid relatedEntityId, string relatedEntityName);
        IQueryable<SupportDocument> GetAllSupportDocumentsQueryable();
        Task<IEnumerable<SupportDocument>> GetAllSupportDocumentsAsync(SupportDocumentTypesDto? supportDocumentTypesDto);
        Task<string?> GetFilePathByIdAsync(Guid fileId);
        Task<bool> FileExistsAsync(string fileName, Guid relatedEntityId, string relatedEntityName);
        Task<bool> AnyFileExistsForEntityIdAsync(Guid relatedEntityId);
        Task<Guid?> GetSupportDocumentIdIfExistsAsync(string fileName, Guid relatedEntityId, string relatedEntityName);
        Task DeleteFileAsync(Guid fileId);
    }
}
