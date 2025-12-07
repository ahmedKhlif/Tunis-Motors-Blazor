using BlazorApp.Models;
using Microsoft.AspNetCore.Components.Forms; // For IBrowserFile

namespace BlazorApp.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto model);
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto model);
        Task<bool> DeleteCategoryAsync(int id);
        Task<List<CarListingDto>> GetCategoryListingsAsync(int categoryId);
        Task<string?> UploadCategoryImageAsync(IBrowserFile file);
    }

    public class CategoryService : ICategoryService
    {
        private readonly IApiClient _apiClient;

        public CategoryService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var response = await _apiClient.GetAsync<List<CategoryDto>>("api/categories");
            return response.Data ?? new List<CategoryDto>();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var response = await _apiClient.GetAsync<CategoryDto>($"api/categories/{id}");
            return response.Data;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto model)
        {
            var response = await _apiClient.PostAsync<CategoryDto>("api/categories", model);
            return response.Data;
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto model)
        {
            var response = await _apiClient.PutAsync<CategoryDto>($"api/categories/{id}", model);
            return response.Data;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/categories/{id}");
            return response.Success;
        }

        public async Task<List<CarListingDto>> GetCategoryListingsAsync(int categoryId)
        {
            var response = await _apiClient.GetAsync<List<CarListingDto>>($"api/categories/{categoryId}/listings");
            return response.Data ?? new List<CarListingDto>();
        }

        public async Task<string?> UploadCategoryImageAsync(IBrowserFile file)
        {
            if (file == null) return null;
            try
            {
                using var content = new MultipartFormDataContent();
                var stream = file.OpenReadStream(5 * 1024 * 1024); // 5MB limit
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.Name);

                var client = new HttpClient { BaseAddress = new Uri(_apiClient.GetBaseUrl()) };
                var token = await _apiClient.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PostAsync("api/categories/upload-image", content);
                var apiResponse = await response.Content.ReadAsStringAsync();
                var parsed = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<FileUploadResponseDto>>(apiResponse, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return parsed?.Data?.FilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryService] Upload error: {ex.Message}");
                return null;
            }
        }
    }
}
