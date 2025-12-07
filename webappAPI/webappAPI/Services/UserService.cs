using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<IdentityUser> userManager, AppDbContext context, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return null;

                var roles = await _userManager.GetRolesAsync(user);
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);

                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles.ToList(),
                    Profile = profile != null ? MapToUserProfileDto(profile) : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user: {ex.Message}");
                return null;
            }
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return null;

                return await GetUserByIdAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by email: {ex.Message}");
                return null;
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new List<string>();

                var roles = await _userManager.GetRolesAsync(user);
                return roles.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user roles: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.AddToRoleAsync(user, role);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error assigning role: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.RemoveFromRoleAsync(user, role);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing role: {ex.Message}");
                return false;
            }
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);

                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        Roles = roles.ToList(),
                        Profile = profile != null ? MapToUserProfileDto(profile) : null
                    });
                }

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all users: {ex.Message}");
                return new List<UserDto>();
            }
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserProfileDto model)
        {
            try
            {
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId);
                if (profile == null)
                    return false;

                profile.FirstName = model.FirstName ?? profile.FirstName;
                profile.LastName = model.LastName ?? profile.LastName;
                profile.PhoneNumber = model.PhoneNumber ?? profile.PhoneNumber;
                profile.Address = model.Address ?? profile.Address;
                profile.City = model.City ?? profile.City;
                profile.Country = model.Country ?? profile.Country;
                profile.DateOfBirth = model.DateOfBirth ?? profile.DateOfBirth;
                profile.ProfilePicture = model.ProfilePicture ?? profile.ProfilePicture;
                profile.Bio = model.Bio ?? profile.Bio;
                profile.UpdatedAt = DateTime.UtcNow;

                _context.UserProfiles.Update(profile);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                return false;
            }
        }

        private UserProfileDto MapToUserProfileDto(UserProfile profile)
        {
            return new UserProfileDto
            {
                Id = profile.Id,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                City = profile.City,
                Country = profile.Country,
                DateOfBirth = profile.DateOfBirth,
                ProfilePicture = profile.ProfilePicture,
                Bio = profile.Bio,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
    }
}
