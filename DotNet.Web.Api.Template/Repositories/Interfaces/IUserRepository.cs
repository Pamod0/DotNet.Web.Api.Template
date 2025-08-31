using ASP.NET_Core_Identity.DTOs;
using ASP.NET_Core_Identity.Models.Auth;
using ASP.NET_Core_Identity.Models.FileUploads;

namespace ASP.NET_Core_Identity.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
        Task<Guid?> GetFirstProfilePicIdAsync(Guid relatedEntityId);
        Task<IEnumerable<ApplicationUser>> GetAllUsersInDepartmentAsync(Guid departmentId);
        Task<IEnumerable<SupportDocument>> GetAllSupportDocumentsAsync(SupportDocumentTypesDto? supportDocumentTypesDto);
    }
}
