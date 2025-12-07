using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(AppDbContext context, ILogger<UserProfileService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            try
            {
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId);
                return profile != null ? MapToDto(profile) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user profile: {ex.Message}");
                return null;
            }
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto model)
        {
            try
            {
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId);
                if (profile == null)
                    return null;

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

                return MapToDto(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user profile: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateUserProfileAsync(string userId)
        {
            try
            {
                var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId);
                if (existingProfile != null)
                    return true;

                var profile = new UserProfile { Id = userId, CreatedAt = DateTime.UtcNow };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user profile: {ex.Message}");
                return false;
            }
        }

        private UserProfileDto MapToDto(UserProfile profile)
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
