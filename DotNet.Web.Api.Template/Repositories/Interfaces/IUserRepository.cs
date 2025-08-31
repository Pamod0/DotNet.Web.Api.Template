using DotNet.Web.Api.Template.DTOs;
using DotNet.Web.Api.Template.Models.Auth;
using DotNet.Web.Api.Template.Models.FileUploads;

namespace DotNet.Web.Api.Template.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
        Task<Guid?> GetFirstProfilePicIdAsync(Guid relatedEntityId);
        Task<IEnumerable<SupportDocument>> GetAllSupportDocumentsAsync(SupportDocumentTypesDto? supportDocumentTypesDto);
    }
}
