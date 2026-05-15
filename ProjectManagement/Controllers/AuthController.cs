using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Common;
using ProjectManagement.DTOs.Auth;
using ProjectManagement.Models.Identity;
using ProjectManagement.Services;
using ProjectManagement.Services.Interfaces;
using System.Security.Cryptography;


namespace ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            JwtService jwtService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "EMAIL_ALREADY_EXISTS",
                    "Email already registered"));
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "REGISTER_FAILED",
                    "Register failed"));
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, Common.RoleConstants.Member);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            var confirmLink =
                $"https://localhost:7076/api/v1/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailAsync(
                user.Email!,
                "Confirm your ProManage account",
                $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2>Welcome to ProManage</h2>
                    <p>Hi {user.FullName},</p>
                    <p>Thanks for registering your account.</p>
                    <p>Please confirm your email address by clicking the button below:</p>
                    <p>
                        <a href='{confirmLink}' 
                           style='display:inline-block;
                                  padding:10px 20px;
                                  background-color:#2563eb;
                                  color:#fff;
                                  text-decoration:none;
                                  border-radius:6px;'>
                            Confirm Email
                        </a>
                    </p>
                    <p>If you did not create this account, you can safely ignore this email.</p>
                    <p>— ProManage Team</p>
                </div>"
            );

            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                "Register successful. Please check your email to confirm your account."));
        }

        // CONFIRM EMAIL
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Redirect("https://localhost:3000/login?verified=false");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return Redirect("https://localhost:3000/login?verified=false");
            }

            return Redirect("https://localhost:3000/login?verified=true");
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "USER_NOT_FOUND",
                    "Email does not exist",
                    401));
            }

            if (!user.EmailConfirmed)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "EMAIL_NOT_CONFIRMED",
                    "Please verify your email before logging in",
                    401));
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isValidPassword)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "INVALID_PASSWORD",
                    "Password is incorrect",
                    401));
            }

            var token = await _jwtService.GenerateToken(user);

            // Generate refresh token and persist it using Identity user tokens
            var refreshToken = GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(7).ToString("o");

            await _userManager.SetAuthenticationTokenAsync(
                user,
                "ProManage",
                "RefreshToken",
                refreshToken);

            await _userManager.SetAuthenticationTokenAsync(
                user,
                "ProManage",
                "RefreshTokenExpiry",
                refreshExpiry);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                token,
                refreshToken
            }, "Login successful"));
        }

        // CHANGE PASSWORD
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "USER_NOT_AUTHENTICATED",
                    "User not found in token",
                    401));
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "CHANGE_PASSWORD_FAILED",
                    "Password change failed"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                "Password changed successfully"));
        }

        // FORGOT PASSWORD
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Ok(ApiResponse<object>.SuccessResponse(
                    null,
                    "If the email exists, a password reset link has been sent."));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            var resetLink =
                $"https://localhost:3000/reset-password?email={user.Email}&token={encodedToken}";

            await _emailService.SendEmailAsync(
                user.Email!,
                "Reset your ProManage password",
                $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2>Password Reset Request</h2>
                    <p>Hi {user.FullName},</p>
                    <p>We received a request to reset your password.</p>
                    <p>Click the button below to set a new password:</p>
                    <p>
                        <a href='{resetLink}'
                           style='display:inline-block;
                                  padding:10px 20px;
                                  background-color:#dc2626;
                                  color:#fff;
                                  text-decoration:none;
                                  border-radius:6px;'>
                            Reset Password
                        </a>
                    </p>
                    <p>If you did not request this, you can safely ignore this email.</p>
                    <p>— ProManage Team</p>
                </div>"
            );

            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                "If the email exists, a password reset link has been sent."));
        }

        // LOGOUT
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto? request)
        {
            // If user is authenticated, revoke stored refresh token for that user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                await _userManager.RemoveAuthenticationTokenAsync(currentUser, "ProManage", "RefreshToken");
                await _userManager.RemoveAuthenticationTokenAsync(currentUser, "ProManage", "RefreshTokenExpiry");

                return Ok(ApiResponse<object>.SuccessResponse(
                    null,
                    "Logged out successfully"));
            }

            // If not authenticated, require email + refresh token to revoke
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "INVALID_REQUEST",
                    "Provide email and refreshToken to logout when not authenticated"));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out (if token existed)"));
            }

            var stored = await _userManager.GetAuthenticationTokenAsync(user, "ProManage", "RefreshToken");
            if (stored != null && stored == request.RefreshToken)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, "ProManage", "RefreshToken");
                await _userManager.RemoveAuthenticationTokenAsync(user, "ProManage", "RefreshTokenExpiry");
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out (if token existed)"));
        }

        // REFRESH TOKEN
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "VALIDATION_ERROR",
                    "Invalid request"));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "INVALID_CREDENTIALS",
                    "Invalid refresh token"));
            }

            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "ProManage", "RefreshToken");
            var storedExpiry = await _userManager.GetAuthenticationTokenAsync(user, "ProManage", "RefreshTokenExpiry");

            if (string.IsNullOrWhiteSpace(storedToken) || storedToken != request.RefreshToken)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "INVALID_REFRESH_TOKEN",
                    "Refresh token is invalid or expired"));
            }

            if (!string.IsNullOrWhiteSpace(storedExpiry) &&
                DateTime.TryParse(storedExpiry, out var expiry) &&
                expiry < DateTime.UtcNow)
            {
                // revoke if expired
                await _userManager.RemoveAuthenticationTokenAsync(user, "ProManage", "RefreshToken");
                await _userManager.RemoveAuthenticationTokenAsync(user, "ProManage", "RefreshTokenExpiry");

                return Unauthorized(ApiResponse<object>.Fail(
                    "REFRESH_TOKEN_EXPIRED",
                    "Refresh token expired"));
            }

            // rotate tokens: issue new JWT + new refresh token
            var newJwt = await _jwtService.GenerateToken(user);
            var newRefresh = GenerateRefreshToken();
            var newExpiry = DateTime.UtcNow.AddDays(7).ToString("o");

            await _userManager.SetAuthenticationTokenAsync(user, "ProManage", "RefreshToken", newRefresh);
            await _userManager.SetAuthenticationTokenAsync(user, "ProManage", "RefreshTokenExpiry", newExpiry);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                token = newJwt,
                refreshToken = newRefresh
            }, "Token refreshed successfully"));
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
    }
}