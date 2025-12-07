using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<IdentityUser> userManager, AppDbContext context, IEmailService emailService, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(model.Email);
                if (userExists != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email is already registered"
                    };
                }

                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User registration failed",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Assign default role
                var role = model.Role ?? "Buyer";
                await _userManager.AddToRoleAsync(user, role);

                // Create user profile with FirstName and LastName
                var profile = new UserProfile
                {
                    Id = user.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();

                // Send confirmation email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"{_configuration["FrontendUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await _emailService.SendEmailConfirmationAsync(user.Email, user.UserName ?? user.Email, confirmationLink);
                _logger.LogInformation($"User {user.Email} registered successfully with role {role}");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "User registered successfully. Please check your email to confirm your account."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Check if account is locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var message = "Your account has been blocked by an administrator. You cannot login until your account is unlocked.";
                    
                    if (lockoutEnd.HasValue && lockoutEnd.Value != DateTimeOffset.MaxValue)
                    {
                        message = $"Your account has been temporarily blocked until {lockoutEnd.Value.LocalDateTime:dd/MM/yyyy HH:mm}. Please contact support for assistance.";
                    }
                    
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = message
                    };
                }

                // Check email confirmation and auto-send verification if not confirmed
                if (!user.EmailConfirmed)
                {
                    // Auto-send verification email
                    try
                    {
                        var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = $"{_configuration["FrontendUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(verificationToken)}";
                        await _emailService.SendEmailConfirmationAsync(user.Email, user.UserName ?? user.Email, confirmationLink);
                        _logger.LogInformation($"Auto-sent verification email to {user.Email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to auto-send verification email: {ex.Message}");
                    }

                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Please confirm your email before logging in. A verification email has been sent to your inbox."
                    };
                }

                var roles = await _userManager.GetRolesAsync(user);
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles.ToList(),
                    Profile = profile != null ? new UserProfileDto
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
                    } : null
                };

                // Generate JWT token
                var token = GenerateJwtToken(user, roles);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.ConfirmEmailAsync(user, token);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResendVerificationEmailAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return true; // Don't reveal if email exists

                if (user.EmailConfirmed)
                    return true; // Already confirmed

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"{_configuration["FrontendUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await _emailService.SendEmailConfirmationAsync(email, user.UserName ?? email, confirmationLink);
                _logger.LogInformation($"Verification email resent to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resending verification email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return true; // Don't reveal if email exists

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = $"{_configuration["FrontendUrl"]}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await _emailService.SendPasswordResetAsync(email, user.UserName ?? email, resetLink);
                _logger.LogInformation($"Password reset email sent to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in forgot password: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? await _userManager.GenerateEmailConfirmationTokenAsync(user) : string.Empty;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null ? await _userManager.GeneratePasswordResetTokenAsync(user) : string.Empty;
        }

        private string GenerateJwtToken(IdentityUser user, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
