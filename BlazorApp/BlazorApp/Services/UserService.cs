using BlazorApp.Models;

namespace BlazorApp.Services
{
    public interface IUserService
    {
        Task<UserDto> GetCurrentUserAsync();
        Task<UserDto> GetUserByIdAsync(string userId);
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<UserProfileDto> GetCurrentUserProfileAsync();
        Task<bool> UpdateProfileAsync(UpdateUserProfileDto model);
        Task<List<UserWithRoleDto>> GetMessageRecipientsAsync();
    }

    public class UserService : IUserService
    {
        private readonly IApiClient _apiClient;

        public UserService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            var response = await _apiClient.GetAsync<UserDto>("api/users/me");
            return response.Data;
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var response = await _apiClient.GetAsync<UserDto>($"api/users/{userId}");
            return response.Data;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var response = await _apiClient.GetAsync<UserProfileDto>($"api/users/profile/{userId}");
            return response.Data;
        }

        public async Task<UserProfileDto> GetCurrentUserProfileAsync()
        {
            var response = await _apiClient.GetAsync<UserProfileDto>("api/users/profile");
            return response.Data;
        }

        public async Task<bool> UpdateProfileAsync(UpdateUserProfileDto model)
        {
            var response = await _apiClient.PutAsync<UserProfileDto>("api/users/profile", model);
            return response.Success;
        }

        public async Task<List<UserWithRoleDto>> GetMessageRecipientsAsync()
        {
            var response = await _apiClient.GetAsync<List<UserWithRoleDto>>("api/messages/recipients");
            return response.Data ?? new List<UserWithRoleDto>();
        }
    }
}
