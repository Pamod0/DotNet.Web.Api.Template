using DotNet.Web.Api.Template.DTOs.User;
using DotNet.Web.Api.Template.Models;

namespace DotNet.Web.Api.Template.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> GetUserByIdAsync(Guid userId);
        Task<PagedResponse<IEnumerable<UserDTO>>> GetAllUsersAsync(PagedRequest request);
        Task<ApiResponse> CreateUserWithRoleAsync(CreateUserDTO createUser);
        Task<ApiResponse> UpdateUserAsync(UpdateUserDto updateUserDto);
        Task<IEnumerable<UserDropdownDto>> GetAllRolesAsync();
        Task<ApiResponse> DeleteUserAsync(Guid userId);
    }
}
