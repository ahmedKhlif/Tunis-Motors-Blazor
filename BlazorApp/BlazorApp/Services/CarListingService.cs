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
        private readonly HttpClient _httpClient;

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
                
                // Ensure baseUrl ends with /
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }
                
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
                
                // Clear any existing headers that might interfere
                httpClient.DefaultRequestHeaders.Clear();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var content = new MultipartFormDataContent();
                var memoryStreams = new List<System.IO.MemoryStream>(); // Keep streams alive until request completes
                
                foreach (var file in fileList)
                {
                    Console.WriteLine($"[UploadImages] Adding file: {file.Name}, Size: {file.Size} bytes, Type: {file.ContentType}");
                    try
                    {
                        // Validate file size before reading
                        if (file.Size > 10 * 1024 * 1024)
                        {
                            Console.WriteLine($"[UploadImages] ERROR: File {file.Name} exceeds 10MB limit (Size: {file.Size} bytes)");
                            continue;
                        }

                        // Validate file extension
                        var fileName = file.Name.ToLowerInvariant();
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var hasValidExtension = allowedExtensions.Any(ext => fileName.EndsWith(ext));
                        if (!hasValidExtension)
                        {
                            Console.WriteLine($"[UploadImages] ERROR: File {file.Name} has invalid extension");
                            continue;
                        }

                        // Read file stream into memory to avoid stream closure issues
                        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                        var memoryStream = new System.IO.MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        memoryStreams.Add(memoryStream); // Keep reference to prevent disposal
                        
                        var fileContent = new StreamContent(memoryStream);
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");
                        
                        // Use "files" as parameter name for IFormFileCollection binding
                        // ASP.NET Core expects files to be named "files" for IFormFileCollection
                        content.Add(fileContent, "files", file.Name);
                        Console.WriteLine($"[UploadImages] File {file.Name} added to multipart content (Size: {file.Size} bytes, Type: {file.ContentType ?? "image/jpeg"})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UploadImages] ERROR adding file {file.Name}: {ex.Message}");
                        Console.WriteLine($"[UploadImages] Stack trace: {ex.StackTrace}");
                    }
                }

                if (content.Count() == 0)
                {
                    Console.WriteLine("[UploadImages] ERROR: No valid files to upload");
                    // Dispose memory streams
                    foreach (var ms in memoryStreams)
                    {
                        ms.Dispose();
                    }
                    return null;
                }

                var uploadUrl = $"{baseUrl}api/carlistings/upload-images";
                Console.WriteLine($"[UploadImages] Posting to: {uploadUrl}");
                Console.WriteLine($"[UploadImages] Content parts count: {content.Count()}");
                Console.WriteLine($"[UploadImages] Content length: {content.Headers.ContentLength?.ToString() ?? "unknown"} bytes");
                
                HttpResponseMessage? response = null;
                try
                {
                    response = await httpClient.PostAsync("api/carlistings/upload-images", content);
                    Console.WriteLine($"[UploadImages] Response status: {response.StatusCode}");
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"[UploadImages] HTTP REQUEST EXCEPTION: {httpEx.Message}");
                    Console.WriteLine($"[UploadImages] Inner exception: {httpEx.InnerException?.Message}");
                    Console.WriteLine($"[UploadImages] Stack trace: {httpEx.StackTrace}");
                    // Dispose memory streams on error
                    foreach (var ms in memoryStreams)
                    {
                        ms.Dispose();
                    }
                    throw; // Re-throw to be caught by outer catch
                }
                catch (TaskCanceledException timeoutEx)
                {
                    Console.WriteLine($"[UploadImages] TIMEOUT EXCEPTION: {timeoutEx.Message}");
                    Console.WriteLine($"[UploadImages] The upload request timed out. Please try with smaller files or check your connection.");
                    // Dispose memory streams on error
                    foreach (var ms in memoryStreams)
                    {
                        ms.Dispose();
                    }
                    throw; // Re-throw to be caught by outer catch
                }
                finally
                {
                    // Dispose memory streams after request completes
                    foreach (var ms in memoryStreams)
                    {
                        ms.Dispose();
                    }
                }
                
                if (response == null)
                {
                    Console.WriteLine("[UploadImages] ERROR: Response is null");
                    return null;
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[UploadImages] Response content: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<string>>>(responseContent, 
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        
                        if (apiResponse?.Data != null && apiResponse.Data.Any())
                        {
                            Console.WriteLine($"[UploadImages] Successfully uploaded {apiResponse.Data.Count} images:");
                            foreach (var path in apiResponse.Data)
                            {
                                Console.WriteLine($"[UploadImages]   - {path}");
                            }
                            return apiResponse.Data;
                        }
                        else
                        {
                            Console.WriteLine("[UploadImages] WARNING: Response data is null or empty");
                            Console.WriteLine($"[UploadImages] API Response Success: {apiResponse?.Success}");
                            Console.WriteLine($"[UploadImages] API Response Message: {apiResponse?.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UploadImages] ERROR deserializing response: {ex.Message}");
                        Console.WriteLine($"[UploadImages] Response content: {responseContent}");
                    }
                }
                else
                {
                    Console.WriteLine($"[UploadImages] ERROR {response.StatusCode}: {responseContent}");
                    
                    // Try to extract error message from response
                    string errorMessage = $"Upload failed with status {response.StatusCode}";
                    try
                    {
                        var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(responseContent, 
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (errorResponse?.Message != null)
                        {
                            errorMessage = errorResponse.Message;
                        }
                    }
                    catch { }
                    
                    Console.WriteLine($"[UploadImages] Error message: {errorMessage}");
                }
                
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[UploadImages] HTTP EXCEPTION: {httpEx.Message}");
                Console.WriteLine($"[UploadImages] Stack trace: {httpEx.StackTrace}");
                return null;
            }
            catch (TaskCanceledException timeoutEx)
            {
                Console.WriteLine($"[UploadImages] TIMEOUT EXCEPTION: {timeoutEx.Message}");
                Console.WriteLine($"[UploadImages] The upload request timed out. Please try with smaller files or check your connection.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadImages] EXCEPTION: {ex.Message}");
                Console.WriteLine($"[UploadImages] Type: {ex.GetType().Name}");
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
