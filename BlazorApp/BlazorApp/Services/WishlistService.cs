using BlazorApp.Models;

namespace BlazorApp.Services
{
    public interface IWishlistService
    {
        Task<List<WishlistDto>> GetMyWishlistAsync();
        Task<bool> AddToWishlistAsync(int productId);
        Task<bool> RemoveFromWishlistAsync(int productId);
        Task<bool> IsInWishlistAsync(int productId);
        Task<ApiResponse> ToggleWishlistAsync(int productId);
    }

    public class WishlistService : IWishlistService
    {
        private readonly IApiClient _apiClient;

        public WishlistService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<WishlistDto>> GetMyWishlistAsync()
        {
            var response = await _apiClient.GetAsync<List<WishlistDto>>("api/wishlist");
            return response.Data ?? new List<WishlistDto>();
        }

        public async Task<bool> AddToWishlistAsync(int productId)
        {
            var createDto = new { ProductId = productId };
            var response = await _apiClient.PostAsync<WishlistDto>("api/wishlist", createDto);
            return response.Success;
        }

        public async Task<bool> RemoveFromWishlistAsync(int productId)
        {
            var response = await _apiClient.DeleteAsync($"api/wishlist/{productId}");
            return response.Success;
        }

        public async Task<bool> IsInWishlistAsync(int productId)
        {
            var response = await _apiClient.GetAsync<bool>($"api/wishlist/check/{productId}");
            return response.Data;
        }

        public async Task<ApiResponse> ToggleWishlistAsync(int productId)
        {
            try
            {
                // First check if it's in wishlist
                var isInWishlist = await IsInWishlistAsync(productId);

                if (isInWishlist)
                {
                    // Remove from wishlist
                    var success = await RemoveFromWishlistAsync(productId);
                    return new ApiResponse
                    {
                        Success = success,
                        Message = success ? "Removed from wishlist" : "Failed to remove from wishlist"
                    };
                }
                else
                {
                    // Add to wishlist
                    var success = await AddToWishlistAsync(productId);
                    return new ApiResponse
                    {
                        Success = success,
                        Message = success ? "Added to wishlist" : "Failed to add to wishlist"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error toggling wishlist: {ex.Message}"
                };
            }
        }
    }
}
