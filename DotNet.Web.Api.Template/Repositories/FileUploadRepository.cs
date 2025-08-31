using DotNet.Web.Api.Template.Data;
using DotNet.Web.Api.Template.DTOs;
using DotNet.Web.Api.Template.Models.FileUploads;
using DotNet.Web.Api.Template.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DotNet.Web.Api.Template.Repositories
{
    public class FileUploadRepository : IFileUploadRepository
    {
        private readonly ApplicationDbContext _context;

        public FileUploadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task SaveFileAsync(SupportDocumentDto supportDocumentDto, Guid relatedEntityId, string relatedEntityName)
        {
            var supportDocument = new SupportDocument
            {
                FileName = supportDocumentDto.FileName,
                FilePath = supportDocumentDto.FilePath,
                ContentType = supportDocumentDto.ContentType,
                FileSize = supportDocumentDto.FileSize,
                Description = supportDocumentDto.Description
            };

            switch (relatedEntityName)
            {
                case "UserProfile":
                    supportDocument.UserProfileID = relatedEntityId;
                    break;
                default:
                    throw new ArgumentException("Invalid related entity name", nameof(relatedEntityName));
            }

            _context.SupportDocuments.Add(supportDocument);
            await _context.SaveChangesAsync();
        }

        public IQueryable<SupportDocument> GetAllSupportDocumentsQueryable()
        {
            return _context.SupportDocuments
                          .Where(m => !m.IsDeleted)
                          .AsQueryable();
        }

        public async Task<IEnumerable<SupportDocument>> GetAllSupportDocumentsAsync(SupportDocumentTypesDto? supportDocumentTypesDto)
        {
            if (supportDocumentTypesDto != null &&
                (!string.IsNullOrWhiteSpace(supportDocumentTypesDto.DecisionId) ||
                 !string.IsNullOrWhiteSpace(supportDocumentTypesDto.TaskId) ||
                 !string.IsNullOrWhiteSpace(supportDocumentTypesDto.MeetingId)))
            {
                return await _context.SupportDocuments
                    .Where(sd => !sd.IsDeleted &&
                        (string.IsNullOrWhiteSpace(supportDocumentTypesDto.UserProfileID) ||
                            (sd.UserProfileID == Guid.Parse(supportDocumentTypesDto.UserProfileID))))
                    .ToListAsync();
            }
            else
            {
                return await _context.SupportDocuments
                    .Where(m => !m.IsDeleted)
                    .ToListAsync();
            }
        }

        public async Task<string?> GetFilePathByIdAsync(Guid fileId)
        {
            var file = await _context.SupportDocuments
                .Where(sd => sd.Id == fileId && !sd.IsDeleted)
                .Select(sd => sd.FilePath)
                .FirstOrDefaultAsync();
            return file;
        }

        public async Task<bool> FileExistsAsync(string fileName, Guid relatedEntityId, string relatedEntityName)
        {
            return await _context.SupportDocuments
                .AnyAsync(sd => sd.FileName == fileName &&
                                !sd.IsDeleted &&
                                (relatedEntityName == "UserProfile" && sd.UserProfileID == relatedEntityId));
        }

        public async Task<bool> AnyFileExistsForEntityIdAsync(Guid relatedEntityId)
        {
            return await _context.SupportDocuments
                .AnyAsync(sd => !sd.IsDeleted &&
                                (sd.UserProfileID == relatedEntityId));
        }

        public async Task<Guid?> GetSupportDocumentIdIfExistsAsync(string fileName, Guid relatedEntityId, string relatedEntityName)
        {
            var supportDocument = await _context.SupportDocuments
                .Where(sd => sd.FileName == fileName &&
                             !sd.IsDeleted &&
                             (relatedEntityName == "UserProfile" && sd.UserProfileID == relatedEntityId))
                .FirstOrDefaultAsync();

            return supportDocument?.Id;
        }

        public async System.Threading.Tasks.Task DeleteFileAsync(Guid fileId)
        {
            var file = await _context.SupportDocuments.FindAsync(fileId);
            if (file != null)
            {
                _context.SupportDocuments.Remove(file);
                await _context.SaveChangesAsync();
            }
        }
    }
}
