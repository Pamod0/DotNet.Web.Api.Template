using ASP.NET_Core_Identity.DTOs.User;
using ASP.NET_Core_Identity.Helpers;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Auth;
using ASP.NET_Core_Identity.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Core_Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailService _emailService;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = user.LockoutEnd,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                Department = user.Department?.Name,

                Role = roles.FirstOrDefault()
            };
        }

        public async Task<PagedResponse<IEnumerable<UserDTO>>> GetAllUsersAsync(PagedRequest request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 10;

            var query = _userManager.Users
                .OrderBy(u => u.Email)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchText) && request.ExactMatch)
            {
                var exactUser = await query.FirstOrDefaultAsync(u => u.Email == request.SearchText);
                if (exactUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(exactUser);
                    var userDto = new UserDTO
                    {
                        Id = exactUser.Id,
                        UserName = exactUser.UserName,
                        FirstName = exactUser.FirstName,
                        LastName = exactUser.LastName,
                        Email = exactUser.Email,
                        EmailConfirmed = exactUser.EmailConfirmed,
                        PhoneNumber = exactUser.PhoneNumber,
                        PhoneNumberConfirmed = exactUser.PhoneNumberConfirmed,
                        TwoFactorEnabled = exactUser.TwoFactorEnabled,
                        LockoutEnd = exactUser.LockoutEnd,
                        LockoutEnabled = exactUser.LockoutEnabled,
                        AccessFailedCount = exactUser.AccessFailedCount,
                        Department = exactUser.Department?.Name,
                        // Assuming a user has only one role for simplicity
                        Role = roles.FirstOrDefault()
                    };
                    return new PagedResponse<IEnumerable<UserDTO>>(1, 1, 1, new List<UserDTO> { userDto });
                }
                return new PagedResponse<IEnumerable<UserDTO>>(1, 1, 0, new List<UserDTO>());
            }

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                query = query.Where(u => u.Email.Contains(request.SearchText));
            }

            var totalRecords = await query.CountAsync();

            var users = await query
                .Include(u => u.Department)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Create a list of UserDTOs to populate
            var userDtos = new List<UserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnd = user.LockoutEnd,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,
                    Department = user.Department?.Name,
                    // Assuming a user has only one role for simplicity, you can also join them if needed
                    Role = roles.FirstOrDefault()
                });
            }

            return new PagedResponse<IEnumerable<UserDTO>>(request.Page, request.PageSize, totalRecords, userDtos);
        }

        public async Task<ApiResponse> CreateUserWithRoleAsync(CreateUserDTO createUser)
        {
            var user = new ApplicationUser
            {
                UserName = createUser.Email,
                Email = createUser.Email,
                FirstName = createUser.FirstName,
                LastName = createUser.LastName,
                DepartmentId = createUser.DepartmentId,
            };

            string password = PasswordGenerator.Generate();

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // set email confirmed to true by default
                user.EmailConfirmed = true;

                if (!await _roleManager.RoleExistsAsync(createUser.Role))
                {
                    return new ApiResponse { Success = false, Message = $"Role '{createUser.Role}' does not exist." };
                }

                var roleResult = await _userManager.AddToRoleAsync(user, createUser.Role);

                if (roleResult.Succeeded)
                {
                    string subject = "Welcome to the Sri Lanka Cricket Decision Management System";
                    string body = $@"
                        <h2>Welcome {createUser.FirstName}!</h2>
                        <p>Your account has been created with the following credentials:</p>
                        <ul>
                            <li>Email: {createUser.Email}</li>
                            <li>Password: {password}</li>
                            <li>Role: {createUser.Role}</li>
                        </ul>
                        <p>Please change your password upon first login.</p>
                    ";

                    await _emailService.SendEmailAsync(createUser.Email, subject, body);
                    //_ = _emailService.SendEmailAsync(createUser.Email, subject, body);

                    return new ApiResponse
                    {
                        Success = true,
                        Message = $"User '{createUser.Email}' created with role '{createUser.Role}' and email sent."
                    };
                }
                else
                {
                    await _userManager.DeleteAsync(user); // Rollback user creation
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to assign role.",
                        Errors = roleResult.Errors.Select(e => e.Description)
                    };
                }
            }
            else
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User creation failed.",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }

        public async Task<ApiResponse> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(updateUserDto.Id.ToString());
            if (user == null)
            {
                return new ApiResponse { Success = false, Message = "User not found." };
            }

            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;
            user.Email = updateUserDto.Email ?? user.Email;
            user.DepartmentId = updateUserDto.DepartmentId ?? user.DepartmentId;

            if (!String.IsNullOrEmpty(updateUserDto.Role))
            {
                if (!await _roleManager.RoleExistsAsync(updateUserDto.Role))
                {
                    return new ApiResponse { Success = false, Message = $"Role '{updateUserDto.Role}' does not exist." };
                }
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to remove user from current roles.",
                        Errors = removeResult.Errors.Select(e => e.Description)
                    };
                }
                var addResult = await _userManager.AddToRoleAsync(user, updateUserDto.Role);
                if (!addResult.Succeeded)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to assign new role to user.",
                        Errors = addResult.Errors.Select(e => e.Description)
                    };
                }
            }

            if (updateUserDto.UserAccountStatus == "active")
            {
                user.LockoutEnd = null;
            }
            else if (updateUserDto.UserAccountStatus == "inactive")
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                user.LockoutEnd = null;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new ApiResponse { Success = true, Message = "User updated successfully." };
            }
            else
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Failed to update user.",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }

        public async Task<IEnumerable<UserDropdownDto>> GetAllRolesAsync()
        {
            if (!_roleManager.Roles.Any())
            {
                return new List<UserDropdownDto>();
            }
            // Return a list of roles as UserDropdownDto
            return _roleManager.Roles
                .Select(r => new UserDropdownDto
                {
                    Id = r.Id,
                    Name = r.Name!
                })
                .ToList();

            //return await Task.FromResult(_roleManager.Roles.Select(r => r.Name!).ToList());
        }
        public async Task<ApiResponse> DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "User deleted successfully."
                };
            }
            else
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Failed to delete user.",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }
        public async Task<string?> GetUserDepartmentAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Department?.Name;
        }

    }
}
