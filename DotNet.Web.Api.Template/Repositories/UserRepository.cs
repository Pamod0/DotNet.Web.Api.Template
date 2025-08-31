using DotNet.Web.Api.Template.Data;
using DotNet.Web.Api.Template.DTOs;
using DotNet.Web.Api.Template.Models.Auth;
using DotNet.Web.Api.Template.Models.FileUploads;
using DotNet.Web.Api.Template.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DotNet.Web.Api.Template.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(user => user.Id == userId && !user.IsDeleted);
        }

        public async Task<Guid?> GetFirstProfilePicIdAsync(Guid relatedEntityId)
        {
            var supportDocument = await _context.SupportDocuments
                .Where(sd => !sd.IsDeleted && (sd.UserProfileID == relatedEntityId))
                .FirstOrDefaultAsync();

            return supportDocument?.Id;
        }

        public async Task<IEnumerable<SupportDocument>> GetAllSupportDocumentsAsync(SupportDocumentTypesDto? supportDocumentTypesDto)
        {
            if (supportDocumentTypesDto != null &&
                (!string.IsNullOrWhiteSpace(supportDocumentTypesDto.UserProfileID)))
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

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersInDepartmentAsync(Guid departmentId)
        {
            return await _context.Users
                .Where(user => !user.IsDeleted && user.DepartmentId == departmentId)
                .ToListAsync();
        }

    }
}
