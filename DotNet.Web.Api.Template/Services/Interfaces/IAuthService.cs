using DotNet.Web.Api.Template.DTOs.Auth;
using DotNet.Web.Api.Template.Models.Auth;
using DotNet.Web.Api.Template.Models.User;
using Microsoft.AspNetCore.Identity;

namespace DotNet.Web.Api.Template.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegistrationResult> RegisterUserAsync(RegisterUserDTO registerUser);
        Task<LoginResult> Login(LoginUser loginUser);
        Task<string> GenerateTokenString(LoginUser loginUser);
        Task<bool> AssignRole(string email, string roleName);
        Task<bool> SendConfirmationEmailAsync(ApplicationUser user);
        Task<bool> ConfirmEmailAsync(Guid userId, string token);
        Task<bool> ResendConfirmationEmailAsync(string email);
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<TwoFactorResponse> EnableTwoFactorAuth(string email);
        Task<bool> VerifyTwoFactorCode(string email, string code, bool rememberDevice);
        Task<bool> DisableTwoFactorAuth(string email);
        Task<bool> VerifyRecoveryCode(string email, string code);
        Task<bool> LockoutUserAsync(Guid userId, TimeSpan? lockoutDuration = null);
        Task<bool> UnlockUserAsync(Guid userId);
    }
}
