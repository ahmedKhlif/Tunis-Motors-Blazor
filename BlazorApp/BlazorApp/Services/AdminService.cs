using BlazorApp.Models;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public interface IAdminService
    {
        Task<List<UserManagementDto>> GetAllUsersAsync();
        Task<UserManagementDto> GetUserByIdAsync(string userId);
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<ApiResponse> CreateRoleAsync(string roleName);
        Task<ApiResponse> DeleteRoleAsync(string roleName);
        Task<ApiResponse> AddUserToRoleAsync(string userId, string roleName);
        Task<ApiResponse> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<ApiResponse> LockUserAsync(string userId);
        Task<ApiResponse> UnlockUserAsync(string userId);
        Task<ApiResponse> DeleteUserAsync(string userId);
        Task<RoleDto> GetRoleByIdAsync(string roleId);
        Task<List<UserDto>> GetUsersInRoleAsync(string roleId);
        Task<ApiResponse> UpdateRoleAsync(string roleId, string roleName);
        Task<List<UserRoleManagementDto>> GetUsersForRoleManagementAsync(string roleId);
        Task<ApiResponse> UpdateUsersInRoleAsync(string roleId, List<UserRoleManagementDto> users);
        Task<ApiResponse> SendVerificationEmailAsync(string userId);
    }

    public class AdminService : IAdminService
    {
        private readonly HttpClient _httpClient;

        public AdminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserManagementDto>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<UserManagementDto>>>("api/admin/users");
                return response?.Data ?? new List<UserManagementDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all users: {ex.Message}");
                return new List<UserManagementDto>();
            }
        }

        public async Task<UserManagementDto> GetUserByIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserManagementDto>>($"api/admin/users/{userId}");
                return response?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<RoleDto>>>("api/admin/roles");
                return response?.Data ?? new List<RoleDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all roles: {ex.Message}");
                return new List<RoleDto>();
            }
        }

        public async Task<ApiResponse> CreateRoleAsync(string roleName)
        {
            try
            {
                var model = new CreateRoleDto { RoleName = roleName };
                var response = await _httpClient.PostAsJsonAsync("api/admin/roles", model);
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating role: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while creating the role" };
            }
        }

        public async Task<ApiResponse> DeleteRoleAsync(string roleName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/admin/roles/{roleName}");
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting role: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while deleting the role" };
            }
        }

        public async Task<ApiResponse> AddUserToRoleAsync(string userId, string roleName)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/admin/users/{userId}/roles/{roleName}", null);
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user to role: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while adding user to role" };
            }
        }

        public async Task<ApiResponse> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/admin/users/{userId}/roles/{roleName}");
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing user from role: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while removing user from role" };
            }
        }

        public async Task<ApiResponse> LockUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/admin/users/{userId}/lock", null);
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error locking user: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while locking the user" };
            }
        }

        public async Task<ApiResponse> UnlockUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/admin/users/{userId}/unlock", null);
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking user: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while unlocking the user" };
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/admin/users/{userId}");
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while deleting the user" };
            }
        }

        public async Task<RoleDto> GetRoleByIdAsync(string roleId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<RoleDetailsDto>>($"api/admin/roles/{roleId}");
                if (response?.Data != null)
                {
                    return new RoleDto
                    {
                        Id = response.Data.Id,
                        Name = response.Data.Name,
                        Description = response.Data.Description,
                        UserCount = response.Data.UserCount
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting role {roleId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserDto>> GetUsersInRoleAsync(string roleId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<RoleDetailsDto>>($"api/admin/roles/{roleId}");
                if (response?.Data?.Users != null)
                {
                    return response.Data.Users.Select(u => new UserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email
                    }).ToList();
                }
                return new List<UserDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users in role {roleId}: {ex.Message}");
                return new List<UserDto>();
            }
        }

        public async Task<ApiResponse> UpdateRoleAsync(string roleId, string roleName)
        {
            try
            {
                var model = new UpdateRoleDto { RoleName = roleName };
                var response = await _httpClient.PutAsJsonAsync($"api/admin/roles/{roleId}", model);
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating role: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while updating the role" };
            }
        }

        public async Task<List<UserRoleManagementDto>> GetUsersForRoleManagementAsync(string roleId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<UserRoleManagementDto>>>($"api/admin/roles/{roleId}/manage-users");
                return response?.Data ?? new List<UserRoleManagementDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users for role management: {ex.Message}");
                return new List<UserRoleManagementDto>();
            }
        }

        public async Task<ApiResponse> UpdateUsersInRoleAsync(string roleId, List<UserRoleManagementDto> users)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/admin/roles/{roleId}/manage-users", users);
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating users in role: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while updating users in role" };
            }
        }

        public async Task<ApiResponse> SendVerificationEmailAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/admin/users/{userId}/send-verification", null);
                return await response.Content.ReadFromJsonAsync<ApiResponse>() ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification email: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while sending verification email" };
            }
        }
    }
}