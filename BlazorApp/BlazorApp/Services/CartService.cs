using BlazorApp.Models;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public class CartService : ICartService
    {
        private readonly IApiClient _apiClient;

        public CartService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<CartDto>> GetCartAsync()
        {
            return await _apiClient.GetAsync<CartDto>("api/cart");
        }

        public async Task<ApiResponse<CartDto>> AddToCartAsync(AddToCartDto model)
        {
            return await _apiClient.PostAsync<CartDto>("api/cart/add", model);
        }

        public async Task<ApiResponse> RemoveFromCartAsync(int itemId)
        {
            return await _apiClient.DeleteAsync($"api/cart/{itemId}");
        }

        public async Task<ApiResponse<CartDto>> UpdateCartItemAsync(int itemId, UpdateCartItemDto model)
        {
            return await _apiClient.PutAsync<CartDto>($"api/cart/{itemId}", model);
        }

        public async Task<ApiResponse> ClearCartAsync()
        {
            return await _apiClient.DeleteAsync("api/cart/clear");
        }

        public async Task<ApiResponse<int>> GetCartCountAsync()
        {
            return await _apiClient.GetAsync<int>("api/cart/count");
        }
    }

    public interface ICartService
    {
        Task<ApiResponse<CartDto>> GetCartAsync();
        Task<ApiResponse<CartDto>> AddToCartAsync(AddToCartDto model);
        Task<ApiResponse> RemoveFromCartAsync(int itemId);
        Task<ApiResponse<CartDto>> UpdateCartItemAsync(int itemId, UpdateCartItemDto model);
        Task<ApiResponse> ClearCartAsync();
        Task<ApiResponse<int>> GetCartCountAsync();
    }
}