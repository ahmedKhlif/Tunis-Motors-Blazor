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
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
        {
            _wishlistService = wishlistService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<WishlistDto>>>> GetMyWishlist()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var wishlist = await _wishlistService.GetUserWishlistAsync(userId);

            return Ok(new ApiResponse<List<WishlistDto>> { Success = true, Data = wishlist });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WishlistDto>>> AddToWishlist([FromBody] AddToWishlistDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var wishlistItem = await _wishlistService.AddToWishlistAsync(userId, model.ProductId);

            if (wishlistItem == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to add to wishlist" });

            return Ok(new ApiResponse<WishlistDto> { Success = true, Data = wishlistItem, Message = "Added to wishlist" });
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult<ApiResponse>> RemoveFromWishlist(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);

            if (!result)
                return NotFound(new ApiResponse { Success = false, Message = "Item not in wishlist" });

            return Ok(new ApiResponse { Success = true, Message = "Removed from wishlist" });
        }

        [HttpGet("check/{productId}")]
        public async Task<ActionResult<ApiResponse<bool>>> IsInWishlist(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId);

            return Ok(new ApiResponse<bool> { Success = true, Data = isInWishlist });
        }
    }
}
