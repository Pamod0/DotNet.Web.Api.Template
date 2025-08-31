using ASP.NET_Core_Identity.DTOs.User;
using ASP.NET_Core_Identity.Models;

namespace ASP.NET_Core_Identity.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> GetUserByIdAsync(Guid userId);
        Task<PagedResponse<IEnumerable<UserDTO>>> GetAllUsersAsync(PagedRequest request);
        Task<ApiResponse> CreateUserWithRoleAsync(CreateUserDTO createUser);
        Task<ApiResponse> UpdateUserAsync(UpdateUserDto updateUserDto);
        Task<IEnumerable<UserDropdownDto>> GetAllRolesAsync();
        Task<ApiResponse> DeleteUserAsync(Guid userId);
        Task<string?> GetUserDepartmentAsync(Guid userId);
    }
}
