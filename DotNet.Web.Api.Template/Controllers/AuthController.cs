using DotNet.Web.Api.Template.DTOs.Auth;
using DotNet.Web.Api.Template.Models;
using DotNet.Web.Api.Template.Models.Auth;
using DotNet.Web.Api.Template.Models.User;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DotNet.Web.Api.Template.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public AuthController(
            IConfiguration config,
            ILogger<AuthController> logger,
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IUserService userService)
        {
            _config = config;
            _logger = logger;
            _authService = authService;
            _userManager = userManager;
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterUserDTO registerUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)

                    });
                }

                var result = await _authService.RegisterUserAsync(registerUser);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred during registration.",
                    Errors = new[] { ex.Message }
                });
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginUser loginUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.Login(loginUser);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred during login.",
                    Errors = new[] { ex.Message }
                });
            }
        }

        [HttpPost("LoginWith2fa")]
        public async Task<ActionResult<AuthResponse>> LoginWith2fa([FromBody] Verify2FACodeRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest("Invalid login attempt.");
            }

            // Verify 2FA code
            var isCodeValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                request.Code);

            if (!isCodeValid)
            {
                return BadRequest("Invalid verification code.");
            }

            // Generate JWT token
            var tokenString = await _authService.GenerateTokenString(new LoginUser
            {
                Email = request.Email,
                Password = "" // Not needed since we already verified the user
            });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResponse
            {
                Token = tokenString,
                Expiration = DateTime.Now.AddMinutes(_config.GetValue<int>("Jwt:ExpireInMinutes")),
                UserId = user.Id,
                Roles = roles.ToList()
            });
        }

        [HttpPost("AssignRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] UserRole userRole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.AssignRole(userRole.Email, userRole.RoleName);
            if (!result)
            {
                return BadRequest("Role assignment failed.");
            }

            return Ok($"Role {userRole.RoleName} assigned to {userRole.Email} successfully.");
        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] EmailConfirmationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ConfirmEmailAsync(request.UserId, request.Token);
            if (!result)
            {
                return BadRequest("Email confirmation failed.");
            }

            return Ok(new ApiResponse { Success = true, Message = "Email confirmed successfully." });
        }

        [HttpPost("ResendConfirmationEmail")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResendConfirmationEmailAsync(request.Email);
            if (!result)
            {
                return BadRequest("Unable to resend confirmation email. The user may not exist or email may already be confirmed.");
            }

            return Ok("Confirmation email resent successfully.");
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _authService.SendPasswordResetEmailAsync(request.Email);

            // Always return success to prevent email enumeration attacks
            return Ok(new
            {
                message = "If your email is registered, you'll receive a password reset link."
            });
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(
                request.Email,
                request.Token,
                request.NewPassword);

            if (!result)
            {
                return BadRequest("Password reset failed. The link may have expired or is invalid.");
            }

            return Ok(new
            {
                message = "Password has been reset successfully."
            });
        }

        [HttpPost("Enable2FA")]
        [Authorize]
        public async Task<ActionResult<TwoFactorResponse>> EnableTwoFactorAuth()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _authService.EnableTwoFactorAuth(email);

            if (!result.Success)
            {
                return BadRequest("Failed to enable 2FA.");
            }

            return Ok(result);
        }

        [HttpPost("Verify2FACode")]
        [Authorize]
        public async Task<IActionResult> VerifyTwoFactorCode([FromBody] Verify2FACodeRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var isValid = await _authService.VerifyTwoFactorCode(email, request.Code, request.RememberDevice);

            if (!isValid)
            {
                return BadRequest("Invalid verification code.");
            }

            // Enable 2FA after successful verification
            var user = await _userManager.FindByEmailAsync(email);
            await _userManager.SetTwoFactorEnabledAsync(user, true);

            return Ok("Two-factor authentication has been enabled successfully.");
        }

        [HttpPost("Disable2FA")]
        [Authorize]
        public async Task<IActionResult> DisableTwoFactorAuth()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var success = await _authService.DisableTwoFactorAuth(email);

            if (!success)
            {
                return BadRequest("Failed to disable 2FA.");
            }

            return Ok("Two-factor authentication has been disabled.");
        }

        [HttpPost("VerifyRecoveryCode")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> VerifyRecoveryCode([FromBody] Verify2FACodeRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest("Invalid request.");
            }

            var isValid = await _authService.VerifyRecoveryCode(request.Email, request.Code);
            if (!isValid)
            {
                return BadRequest("Invalid recovery code.");
            }

            // Generate JWT token
            var tokenString = await _authService.GenerateTokenString(new LoginUser
            {
                Email = request.Email,
                Password = "" // Not needed since we're using recovery code
            });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResponse
            {
                Token = tokenString,
                Expiration = DateTime.Now.AddMinutes(_config.GetValue<int>("Jwt:ExpireInMinutes")),
                UserId = user.Id,
                Roles = roles.ToList()
            });
        }

        [HttpPost("LockoutUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockoutUser([FromBody] LockoutUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _authService.LockoutUserAsync(request.UserId, request.LockoutDuration);
            if (!success)
            {
                return BadRequest("Failed to lockout the user. The user may not exist.");
            }

            return Ok("User has been locked out successfully.");
        }

        [HttpPost("UnlockUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockUser([FromBody] Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            var success = await _authService.UnlockUserAsync(userId);
            if (!success)
            {
                return BadRequest("Failed to unlock the user. The user may not exist.");
            }

            return Ok("User has been unlocked successfully.");
        }
    }
}
