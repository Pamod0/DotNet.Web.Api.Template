using ASP.NET_Core_Identity.Data;
using ASP.NET_Core_Identity.DTOs;
using ASP.NET_Core_Identity.Models.Decisions;
using ASP.NET_Core_Identity.Models.FileUploads;
using ASP.NET_Core_Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Core_Identity.Repositories
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
                case "Decision":
                    supportDocument.DecisionId = relatedEntityId;
                    break;
                case "Task":
                    supportDocument.TaskId = relatedEntityId;
                    break;
                case "Meeting":
                    supportDocument.MeetingId = relatedEntityId;
                    break;
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
                        (string.IsNullOrWhiteSpace(supportDocumentTypesDto.DecisionId) ||
                            (sd.DecisionId != null && sd.DecisionId == Guid.Parse(supportDocumentTypesDto.DecisionId))) &&
                        (string.IsNullOrWhiteSpace(supportDocumentTypesDto.TaskId) ||
                            (sd.TaskId != null && sd.TaskId == Guid.Parse(supportDocumentTypesDto.TaskId))) &&
                        (string.IsNullOrWhiteSpace(supportDocumentTypesDto.MeetingId) ||
                            (sd.MeetingId != null && sd.MeetingId == Guid.Parse(supportDocumentTypesDto.MeetingId))) &&
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
                                (relatedEntityName == "Decision" && sd.DecisionId == relatedEntityId ||
                                 relatedEntityName == "Task" && sd.TaskId == relatedEntityId ||
                                 relatedEntityName == "Meeting" && sd.MeetingId == relatedEntityId ||
                                 relatedEntityName == "UserProfile" && sd.UserProfileID == relatedEntityId));
        }

        public async Task<bool> AnyFileExistsForEntityIdAsync(Guid relatedEntityId)
        {
            return await _context.SupportDocuments
                .AnyAsync(sd => !sd.IsDeleted &&
                                (sd.DecisionId == relatedEntityId ||
                                 sd.TaskId == relatedEntityId ||
                                 sd.MeetingId == relatedEntityId ||
                                 sd.UserProfileID == relatedEntityId));
        }

        public async Task<Guid?> GetSupportDocumentIdIfExistsAsync(string fileName, Guid relatedEntityId, string relatedEntityName)
        {
            var supportDocument = await _context.SupportDocuments
                .Where(sd => sd.FileName == fileName &&
                             !sd.IsDeleted &&
                             (relatedEntityName == "Decision" && sd.DecisionId == relatedEntityId ||
                              relatedEntityName == "Task" && sd.TaskId == relatedEntityId ||
                              relatedEntityName == "Meeting" && sd.MeetingId == relatedEntityId ||
                              relatedEntityName == "UserProfile" && sd.UserProfileID == relatedEntityId))
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
