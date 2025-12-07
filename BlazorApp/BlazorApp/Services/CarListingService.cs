using BlazorApp.Models;

namespace BlazorApp.Services
{
    public interface ICarListingService
    {
        Task<PaginatedDto<CarListingDto>> GetAllListingsAsync(CarListingFilterDto filter);
        Task<CarListingDto> GetListingByIdAsync(int id);
        Task<List<CarListingDto>> GetListingsByCategoryAsync(int categoryId);
        Task<List<CarListingDto>> GetUserListingsAsync(string sellerId);
        Task<CarListingDto> CreateListingAsync(CreateCarListingDto model);
        Task<CarListingDto> UpdateListingAsync(int id, UpdateCarListingDto model);
        Task<bool> DeleteListingAsync(int id);
        Task<List<string>> GetBrandsAsync();
        Task<List<string>> GetFuelTypesAsync();
        Task<List<string>> GetTransmissionsAsync();
        Task<List<string>> GetColorsAsync();
        Task<ApiResponse> DeleteCarAsync(int id);
        Task<List<string>?> UploadImagesAsync(IEnumerable<Microsoft.AspNetCore.Components.Forms.IBrowserFile> files);
        Task<bool> IncrementViewsAsync(int id);
    }

    public class CarListingService : ICarListingService
    {
        private readonly IApiClient _apiClient;

        public CarListingService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PaginatedDto<CarListingDto>> GetAllListingsAsync(CarListingFilterDto filter)
        {
            var response = await _apiClient.GetAsync<PaginatedDto<CarListingDto>>(
                $"api/carlistings?searchTerm={filter.SearchTerm}&categoryId={filter.CategoryId}&brand={filter.Brand}&minPrice={filter.MinPrice}&maxPrice={filter.MaxPrice}&page={filter.Page}&pageSize={filter.PageSize}");
            return response.Data ?? new PaginatedDto<CarListingDto>();
        }

        public async Task<CarListingDto> GetListingByIdAsync(int id)
        {
            var response = await _apiClient.GetAsync<CarListingDto>($"api/carlistings/{id}");
            return response.Data;
        }

        public async Task<List<CarListingDto>> GetListingsByCategoryAsync(int categoryId)
        {
            var response = await _apiClient.GetAsync<List<CarListingDto>>($"api/carlistings/category/{categoryId}");
            return response.Data ?? new List<CarListingDto>();
        }

        public async Task<List<CarListingDto>> GetUserListingsAsync(string sellerId)
        {
            var response = await _apiClient.GetAsync<List<CarListingDto>>($"api/carlistings/seller/{sellerId}");
            return response.Data ?? new List<CarListingDto>();
        }

        public async Task<CarListingDto> CreateListingAsync(CreateCarListingDto model)
        {
            var response = await _apiClient.PostAsync<CarListingDto>("api/carlistings", model);
            return response.Data;
        }

        public async Task<CarListingDto> UpdateListingAsync(int id, UpdateCarListingDto model)
        {
            var response = await _apiClient.PutAsync<CarListingDto>($"api/carlistings/{id}", model);
            return response.Data;
        }

        public async Task<bool> DeleteListingAsync(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/carlistings/{id}");
            return response.Success;
        }

        public async Task<List<string>> GetBrandsAsync()
        {
            var response = await _apiClient.GetAsync<List<string>>("api/carlistings/filters/brands");
            return response.Data ?? new List<string>();
        }

        public async Task<List<string>> GetFuelTypesAsync()
        {
            var response = await _apiClient.GetAsync<List<string>>("api/carlistings/filters/fuel-types");
            return response.Data ?? new List<string>();
        }

        public async Task<List<string>> GetTransmissionsAsync()
        {
            var response = await _apiClient.GetAsync<List<string>>("api/carlistings/filters/transmissions");
            return response.Data ?? new List<string>();
        }

        public async Task<List<string>> GetColorsAsync()
        {
            var response = await _apiClient.GetAsync<List<string>>("api/carlistings/filters/colors");
            return response.Data ?? new List<string>();
        }

        public async Task<ApiResponse> DeleteCarAsync(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/carlistings/{id}");
            return new ApiResponse
            {
                Success = response.Success,
                Message = response.Message
            };
        }

        public async Task<List<string>?> UploadImagesAsync(IEnumerable<Microsoft.AspNetCore.Components.Forms.IBrowserFile> files)
        {
            try
            {
                var baseUrl = _apiClient.GetBaseUrl();
                Console.WriteLine($"[UploadImages] Starting upload. Base URL: {baseUrl}");
                
                if (string.IsNullOrEmpty(baseUrl))
                {
                    Console.WriteLine("[UploadImages] ERROR: Base URL is empty");
                    return null;
                }

                var fileList = files.ToList();
                Console.WriteLine($"[UploadImages] Processing {fileList.Count} files");
                
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.Timeout = TimeSpan.FromMinutes(5); // Increase timeout for large files
                
                // Add auth token
                var token = await _apiClient.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("[UploadImages] Auth token added");
                }
                else
                {
                    Console.WriteLine("[UploadImages] WARNING: No auth token available");
                }

                using var content = new MultipartFormDataContent();
                
                foreach (var file in fileList)
                {
                    Console.WriteLine($"[UploadImages] Adding file: {file.Name}, Size: {file.Size} bytes, Type: {file.ContentType}");
                    try
                    {
                        var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        content.Add(fileContent, "files", file.Name);
                        Console.WriteLine($"[UploadImages] File {file.Name} added to multipart content");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UploadImages] ERROR adding file {file.Name}: {ex.Message}");
                    }
                }

                var uploadUrl = $"{baseUrl}api/carlistings/upload-images";
                Console.WriteLine($"[UploadImages] Posting to: {uploadUrl}");
                Console.WriteLine($"[UploadImages] Content length: {content.Headers.ContentLength} bytes");
                
                var response = await httpClient.PostAsync("api/carlistings/upload-images", content);
                
                Console.WriteLine($"[UploadImages] Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[UploadImages] Success response: {jsonResponse}");
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<string>>>(jsonResponse, 
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (apiResponse?.Data != null)
                    {
                        Console.WriteLine($"[UploadImages] Successfully uploaded {apiResponse.Data.Count} images:");
                        foreach (var path in apiResponse.Data)
                        {
                            Console.WriteLine($"[UploadImages]   - {path}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[UploadImages] WARNING: Response data is null");
                    }
                    
                    return apiResponse?.Data;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[UploadImages] ERROR {response.StatusCode}: {errorContent}");
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadImages] EXCEPTION: {ex.Message}");
                Console.WriteLine($"[UploadImages] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> IncrementViewsAsync(int id)
        {
            try
            {
                var response = await _apiClient.PostAsync<object>($"api/carlistings/{id}/increment-view", null);
                return response.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error incrementing views: {ex.Message}");
                return false;
            }
        }
    }
}
