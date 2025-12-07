using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webappAPI.DTOs;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public AccessController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("denied")]
        public IActionResult AccessDenied([FromQuery] bool returnHtml = false)
        {
            // If requesting HTML view (for browser navigation)
            if (returnHtml)
            {
                var htmlPath = Path.Combine(_env.WebRootPath, "access-denied.html");
                if (System.IO.File.Exists(htmlPath))
                {
                    var htmlContent = System.IO.File.ReadAllText(htmlPath);
                    return Content(htmlContent, "text/html");
                }
            }

            // Default JSON response for API calls
            return StatusCode(403, new ApiResponse
            {
                Success = false,
                Message = "Access Denied. You do not have permission to access this resource. Please contact your administrator if you believe this is an error."
            });
        }

        [HttpGet("check-auth")]
        [Authorize]
        public ActionResult<ApiResponse<object>> CheckAuth()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Authenticated successfully",
                Data = new
                {
                    UserId = userId,
                    Email = email,
                    UserName = userName,
                    Roles = roles,
                    AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                }
            });
        }
    }
}
