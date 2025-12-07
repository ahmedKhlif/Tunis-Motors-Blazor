using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webappAPI.DTOs;
using webappAPI.Services;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IUserProfileService userProfileService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _userProfileService = userProfileService;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found" });

            return Ok(new ApiResponse<UserDto> { Success = true, Data = user });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found" });

            return Ok(new ApiResponse<UserDto> { Success = true, Data = user });
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found" });

            return Ok(new ApiResponse<UserDto> { Success = true, Data = user });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new ApiResponse<List<UserDto>> { Success = true, Data = users });
        }

        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile([FromBody] UpdateUserProfileDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _userProfileService.UpdateUserProfileAsync(userId, model);

            if (profile == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to update profile" });

            return Ok(new ApiResponse<UserProfileDto> { Success = true, Data = profile, Message = "Profile updated successfully" });
        }

        [HttpGet("profile/{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile(string userId)
        {
            var profile = await _userProfileService.GetUserProfileAsync(userId);
            if (profile == null)
                return NotFound(new ApiResponse { Success = false, Message = "Profile not found" });

            return Ok(new ApiResponse<UserProfileDto> { Success = true, Data = profile });
        }
    }
}
