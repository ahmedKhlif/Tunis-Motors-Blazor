using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webappAPI.DTOs;
using webappAPI.Services;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IUserProfileService userProfileService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _userProfileService = userProfileService;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input" });

            var result = await _authService.RegisterAsync(model);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input" });

            var result = await _authService.LoginAsync(model);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid parameters" });

            var result = await _authService.ConfirmEmailAsync(userId, token);
            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Email confirmation failed" });

            return Ok(new ApiResponse { Success = true, Message = "Email confirmed successfully" });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> ForgotPassword([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new ApiResponse { Success = false, Message = "Email is required" });

            var result = await _authService.ForgotPasswordAsync(email);
            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Error processing forgot password" });

            return Ok(new ApiResponse { Success = true, Message = "Check your email for password reset instructions" });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> ResetPassword([FromQuery] string userId, [FromQuery] string token, [FromBody] ResetPasswordDto model)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(model.NewPassword))
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid parameters" });

            var result = await _authService.ResetPasswordAsync(userId, token, model.NewPassword);
            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Password reset failed" });

            return Ok(new ApiResponse { Success = true, Message = "Password reset successfully" });
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> ResendVerification([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new ApiResponse { Success = false, Message = "Email is required" });

            var result = await _authService.ResendVerificationEmailAsync(email);
            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Error resending verification email" });

            return Ok(new ApiResponse { Success = true, Message = "Verification email sent successfully" });
        }
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; }
    }
}
