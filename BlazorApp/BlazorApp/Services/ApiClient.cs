using System.Net.Http.Json;
using BlazorApp.Models;
using Blazored.LocalStorage;

namespace BlazorApp.Services
{
    public interface IApiClient
    {
        Task<ApiResponse<T>> GetAsync<T>(string endpoint);
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null);
        Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null);
        Task<ApiResponse> DeleteAsync(string endpoint);
        Task SetTokenAsync(string token);
        Task RemoveTokenAsync();
        string GetBaseUrl();
        Task<string?> GetTokenAsync();
    }

    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private const string TokenKey = "authToken";

        public ApiClient(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                await SetAuthorizationHeader();
                HttpResponseMessage response;
                
                if (data != null)
                {
                    response = await _httpClient.PostAsJsonAsync(endpoint, data);
                }
                else
                {
                    response = await _httpClient.PostAsync(endpoint, null);
                }

                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<ApiResponse> DeleteAsync(string endpoint)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponse(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task SetTokenAsync(string token)
        {
            await _localStorage.SetItemAsync(TokenKey, token);
        }

        public async Task RemoveTokenAsync()
        {
            await _localStorage.RemoveItemAsync(TokenKey);
        }

        public string GetBaseUrl()
        {
            return _httpClient.BaseAddress?.ToString() ?? "";
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>(TokenKey);
        }

        private async Task SetAuthorizationHeader()
        {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            try
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ApiClient] Response Status: {response.StatusCode}");
                Console.WriteLine($"[ApiClient] Response Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    // First try to deserialize as wrapped ApiResponse<T>
                    try
                    {
                        var wrappedContent = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<T>>(
                            responseBody, 
                            new System.Text.Json.JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true 
                            });
                        
                        if (wrappedContent != null && wrappedContent.Data != null)
                        {
                            Console.WriteLine($"[ApiClient] Successfully deserialized as ApiResponse<{typeof(T).Name}>");
                            return wrappedContent;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ApiClient] Failed to deserialize as wrapped: {ex.Message}");
                    }

                    // Fallback: try direct deserialization
                    var directContent = System.Text.Json.JsonSerializer.Deserialize<T>(
                        responseBody, 
                        new System.Text.Json.JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });
                    
                    if (directContent != null)
                    {
                        Console.WriteLine($"[ApiClient] Deserialized directly as {typeof(T).Name}");
                        return new ApiResponse<T> { Success = true, Data = directContent };
                    }

                    return new ApiResponse<T> { Success = true };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await RemoveTokenAsync();
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = "Unauthorized. Please login again."
                    };
                }
                else
                {
                    // Non-success status: attempt flexible error parsing (supports ProblemDetails and custom wrappers)
                    var raw = await response.Content.ReadAsStringAsync();
                    try
                    {
                        // Try ApiResponse first
                        var apiError = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (apiError != null && (apiError.Message != null || apiError.Errors != null))
                        {
                            return new ApiResponse<T>
                            {
                                Success = false,
                                Message = apiError.Message ?? "Request failed",
                                Errors = apiError.Errors
                            };
                        }
                    }
                    catch { /* swallow and fallback */ }

                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(raw);
                        var root = doc.RootElement;
                        string message = root.TryGetProperty("title", out var titleEl) ? titleEl.GetString() ?? "Validation error" : "Request failed";
                        var errorsList = new List<string>();
                        if (root.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            foreach (var prop in errorsEl.EnumerateObject())
                            {
                                foreach (var item in prop.Value.EnumerateArray())
                                {
                                    errorsList.Add(item.GetString() ?? string.Empty);
                                }
                            }
                        }
                        return new ApiResponse<T>
                        {
                            Success = false,
                            Message = message,
                            Errors = errorsList.Count > 0 ? errorsList : null
                        };
                    }
                    catch (Exception ex)
                    {
                        return new ApiResponse<T>
                        {
                            Success = false,
                            Message = $"An error occurred (unparsable response): {ex.Message}"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] HandleResponse Error: {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Error parsing response: {ex.Message}"
                };
            }
        }

        private async Task<ApiResponse> HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return new ApiResponse { Success = true };
                try
                {
                    var content = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return content ?? new ApiResponse { Success = true };
                }
                catch
                {
                    return new ApiResponse { Success = true, Message = null };
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RemoveTokenAsync();
                return new ApiResponse
                {
                    Success = false,
                    Message = "Unauthorized. Please login again."
                };
            }
            else
            {
                var raw = await response.Content.ReadAsStringAsync();
                // Attempt flexible parsing similar to generic handler
                try
                {
                    var apiError = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiError != null) return new ApiResponse { Success = false, Message = apiError.Message, Errors = apiError.Errors };
                }
                catch { }

                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(raw);
                    var root = doc.RootElement;
                    string message = root.TryGetProperty("title", out var titleEl) ? titleEl.GetString() ?? "Request failed" : "Request failed";
                    var errorsList = new List<string>();
                    if (root.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        foreach (var prop in errorsEl.EnumerateObject())
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                errorsList.Add(item.GetString() ?? string.Empty);
                            }
                        }
                    }
                    return new ApiResponse { Success = false, Message = message, Errors = errorsList.Count > 0 ? errorsList : null };
                }
                catch (Exception ex)
                {
                    return new ApiResponse { Success = false, Message = $"Unparsable error response: {ex.Message}" };
                }
            }
        }
    }
}
