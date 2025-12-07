using BlazorApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System;

namespace BlazorApp.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterModel model);
        Task<AuthResponseDto> LoginAsync(LoginModel model);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<UserDto> GetCurrentUserAsync();
        Task<AuthResponseDto> ResetPasswordAsync(string userId, string token, string password);
        Task<AuthResponseDto> ForgotPasswordAsync(string email);
        Task<AuthResponseDto> ResendVerificationAsync(string email);
        Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token);
    }

    public class AuthService : IAuthService
    {
        private readonly IApiClient _apiClient;
        private readonly CustomAuthStateProvider _authStateProvider;

        public AuthService(IApiClient apiClient, AuthenticationStateProvider authStateProvider)
        {
            _apiClient = apiClient;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterModel model)
        {
            var registerDto = new { Email = model.Email, Password = model.Password, ConfirmPassword = model.Password, Role = model.Role };
            var response = await _apiClient.PostAsync<AuthResponseDto>("api/auth/register", registerDto);
            return response.Data ?? new AuthResponseDto { Success = false, Message = response.Message };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginModel model)
        {
            var loginDto = new { model.Email, model.Password };
            Console.WriteLine($"[AuthService] Sending login request for: {model.Email}");
            
            var response = await _apiClient.PostAsync<AuthResponseDto>("api/auth/login", loginDto);
            
            Console.WriteLine($"[AuthService] API Response - Success: {response?.Data?.Success}");
            Console.WriteLine($"[AuthService] API Response - Message: {response?.Data?.Message}");
            Console.WriteLine($"[AuthService] API Response - Token Length: {response?.Data?.Token?.Length ?? 0}");

            if (response.Data?.Success == true && response.Data.Token != null)
            {
                Console.WriteLine("[AuthService] Marking user as authenticated...");
                await _authStateProvider.MarkUserAsAuthenticated(response.Data.Token);
                Console.WriteLine("[AuthService] User marked as authenticated");
            }
            else
            {
                Console.WriteLine($"[AuthService] Login failed - Success: {response.Data?.Success}, Has Token: {response.Data?.Token != null}");
            }

            return response.Data ?? new AuthResponseDto { Success = false, Message = response.Message };
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.MarkUserAsLoggedOut();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                return user != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            var response = await _apiClient.GetAsync<UserDto>("api/users/me");
            return response.Data;
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(string userId, string token, string password)
        {
            var resetDto = new { NewPassword = password };
            var response = await _apiClient.PostAsync<AuthResponseDto>($"api/auth/reset-password?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}", resetDto);
            return response.Data ?? new AuthResponseDto { Success = false, Message = response.Message };
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(string email)
        {
            var response = await _apiClient.PostAsync<AuthResponseDto>($"api/auth/forgot-password?email={Uri.EscapeDataString(email)}", null);
            return response.Data ?? new AuthResponseDto { Success = false, Message = response.Message };
        }

        public async Task<AuthResponseDto> ResendVerificationAsync(string email)
        {
            var response = await _apiClient.PostAsync<AuthResponseDto>($"api/auth/resend-verification?email={Uri.EscapeDataString(email)}", null);
            return response.Data ?? new AuthResponseDto { Success = false, Message = response.Message };
        }

        public async Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token)
        {
            var response = await _apiClient.PostAsync<AuthResponseDto>($"api/auth/confirm-email?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}", null);
            return response.Data ?? new AuthResponseDto { Success = false, Message = response.Message };
        }
    }
}
