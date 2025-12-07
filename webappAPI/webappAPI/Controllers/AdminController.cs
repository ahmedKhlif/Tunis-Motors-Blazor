using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Services;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AdminController(
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            ILogger<AdminController> logger,
            AppDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
            _emailService = emailService;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<UserManagementDto>>>> GetAllUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();
                var userDtos = new List<UserManagementDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);
                    
                    // Count listings and orders
                    var listingsCount = await _context.CarListings.CountAsync(c => c.SellerId == user.Id);
                    var ordersCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);

                    userDtos.Add(new UserManagementDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = profile != null ? $"{profile.FirstName} {profile.LastName}" : "",
                        PhoneNumber = user.PhoneNumber ?? profile?.PhoneNumber ?? "",
                        EmailConfirmed = user.EmailConfirmed,
                        Roles = roles.ToList(),
                        IsLocked = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow,
                        IsActive = lockoutEnd == null || lockoutEnd.Value <= DateTimeOffset.UtcNow,
                        CreatedAt = profile?.CreatedAt.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        ListingsCount = listingsCount,
                        OrdersCount = ordersCount
                    });
                }

                return Ok(new ApiResponse<List<UserManagementDto>> 
                { 
                    Success = true, 
                    Data = userDtos,
                    Message = $"{userDtos.Count} users found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting users: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving users" });
            }
        }

        // GET: api/admin/users/{userId}
        [HttpGet("users/{userId}")]
        public async Task<ActionResult<ApiResponse<UserManagementDto>>> GetUserById(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });

                var roles = await _userManager.GetRolesAsync(user);
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);
                
                // Count listings and orders
                var listingsCount = await _context.CarListings.CountAsync(c => c.SellerId == user.Id);
                var ordersCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);

                var userDto = new UserManagementDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = profile != null ? $"{profile.FirstName} {profile.LastName}" : "",
                    PhoneNumber = user.PhoneNumber ?? profile?.PhoneNumber ?? "",
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList(),
                    IsLocked = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow,
                    IsActive = lockoutEnd == null || lockoutEnd.Value <= DateTimeOffset.UtcNow,
                    CreatedAt = profile?.CreatedAt.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    ListingsCount = listingsCount,
                    OrdersCount = ordersCount
                };

                return Ok(new ApiResponse<UserManagementDto> { Success = true, Data = userDto });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user {userId}: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving user" });
            }
        }

        // GET: api/admin/roles
        [HttpGet("roles")]
        public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAllRoles()
        {
            try
            {
                var roles = _roleManager.Roles.ToList();
                var roleDtos = new List<RoleDto>();

                foreach (var role in roles)
                {
                    // Count users in this role
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                    
                    roleDtos.Add(new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = "", // No description in IdentityRole
                        UserCount = usersInRole.Count
                    });
                }

                return Ok(new ApiResponse<List<RoleDto>>
                {
                    Success = true,
                    Data = roleDtos,
                    Message = $"{roleDtos.Count} roles found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting roles: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving roles" });
            }
        }

        // GET: api/admin/roles/{roleId}
        [HttpGet("roles/{roleId}")]
        public async Task<ActionResult<ApiResponse<RoleDetailsDto>>> GetRoleById(string roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Role not found" });

                // Get all users in this role
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                var userDtos = new List<UserManagementDto>();

                foreach (var user in usersInRole)
                {
                    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var roles = await _userManager.GetRolesAsync(user);
                    var listingsCount = await _context.CarListings.CountAsync(c => c.SellerId == user.Id);
                    var ordersCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);

                    userDtos.Add(new UserManagementDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = profile != null ? $"{profile.FirstName} {profile.LastName}" : "",
                        PhoneNumber = user.PhoneNumber ?? profile?.PhoneNumber ?? "",
                        EmailConfirmed = user.EmailConfirmed,
                        Roles = roles.ToList(),
                        IsLocked = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow,
                        IsActive = lockoutEnd == null || lockoutEnd.Value <= DateTimeOffset.UtcNow,
                        CreatedAt = profile?.CreatedAt.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        ListingsCount = listingsCount,
                        OrdersCount = ordersCount
                    });
                }

                var roleDetails = new RoleDetailsDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = "",
                    UserCount = usersInRole.Count,
                    Users = userDtos
                };

                return Ok(new ApiResponse<RoleDetailsDto> { Success = true, Data = roleDetails });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting role {roleId}: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving role" });
            }
        }

        // POST: api/admin/roles
        [HttpPost("roles")]
        public async Task<ActionResult<ApiResponse>> CreateRole([FromBody] CreateRoleDto model)
        {
            if (string.IsNullOrWhiteSpace(model.RoleName))
                return BadRequest(new ApiResponse { Success = false, Message = "Role name is required" });

            try
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
                if (roleExists)
                    return BadRequest(new ApiResponse { Success = false, Message = "Role already exists" });

                var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role '{model.RoleName}' created successfully");
                    return Ok(new ApiResponse { Success = true, Message = $"Role '{model.RoleName}' created successfully" });
                }

                return BadRequest(new ApiResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating role: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error creating role" });
            }
        }

        // DELETE: api/admin/roles/{roleName}
        [HttpDelete("roles/{roleName}")]
        public async Task<ActionResult<ApiResponse>> DeleteRole(string roleName)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Role not found" });

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role '{roleName}' deleted successfully");
                    return Ok(new ApiResponse { Success = true, Message = $"Role '{roleName}' deleted successfully" });
                }

                return BadRequest(new ApiResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting role: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error deleting role" });
            }
        }

        // POST: api/admin/users/{userId}/roles/{roleName}
        [HttpPost("users/{userId}/roles/{roleName}")]
        public async Task<ActionResult<ApiResponse>> AddUserToRole(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                    return NotFound(new ApiResponse { Success = false, Message = "Role not found" });

                var isInRole = await _userManager.IsInRoleAsync(user, roleName);
                if (isInRole)
                    return BadRequest(new ApiResponse { Success = false, Message = "User is already in this role" });

                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User '{user.UserName}' added to role '{roleName}'");
                    return Ok(new ApiResponse { Success = true, Message = $"User added to role '{roleName}' successfully" });
                }

                return BadRequest(new ApiResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding user to role: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error adding user to role" });
            }
        }

        // DELETE: api/admin/users/{userId}/roles/{roleName}
        [HttpDelete("users/{userId}/roles/{roleName}")]
        public async Task<ActionResult<ApiResponse>> RemoveUserFromRole(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });

                var isInRole = await _userManager.IsInRoleAsync(user, roleName);
                if (!isInRole)
                    return BadRequest(new ApiResponse { Success = false, Message = "User is not in this role" });

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User '{user.UserName}' removed from role '{roleName}'");
                    return Ok(new ApiResponse { Success = true, Message = $"User removed from role '{roleName}' successfully" });
                }

                return BadRequest(new ApiResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing user from role: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error removing user from role" });
            }
        }

        // POST: api/admin/users/{userId}/lock
        [HttpPost("users/{userId}/lock")]
        public async Task<ActionResult<ApiResponse>> LockUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });

                var lockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Lock for 100 years
                var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                
                if (result.Succeeded)
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    _logger.LogInformation($"User '{user.UserName}' locked until {lockoutEnd}");

                    // Send email notification
                    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);
                    var userName = profile != null ? profile.FirstName : user.UserName;
                    
                    var emailSubject = "Account Locked - Tunisia Motors";
                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <div style='text-align: center; margin-bottom: 20px;'>
                                <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 200px; height: auto;' />
                            </div>
                            <h2 style='color: #dc2626;'>Account Locked</h2>
                            <p>Dear {userName},</p>
                            <p>Your Tunisia Motors account has been locked by an administrator.</p>
                            
                            <div style='background: #fee; padding: 20px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #dc2626;'>
                                <p><strong>Account Status:</strong> Locked</p>
                                <p><strong>Email:</strong> {user.Email}</p>
                                <p><strong>Date:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                            </div>

                            <p>You will not be able to login to your account until it is unlocked by an administrator.</p>
                            <p>If you believe this is an error, please contact our support team.</p>

                            <p>Best regards,<br>Tunisia Motors Team</p>
                        </div>";

                    await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
                    
                    return Ok(new ApiResponse { Success = true, Message = "User locked successfully and notification email sent" });
                }

                return BadRequest(new ApiResponse { Success = false, Message = "Failed to lock user" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error locking user: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error locking user" });
            }
        }

        // POST: api/admin/users/{userId}/unlock
        [HttpPost("users/{userId}/unlock")]
        public async Task<ActionResult<ApiResponse>> UnlockUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });

                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                
                if (result.Succeeded)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                    _logger.LogInformation($"User '{user.UserName}' unlocked");

                    // Send welcome back email notification
                    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);
                    var userName = profile != null ? profile.FirstName : user.UserName;
                    
                    var emailSubject = "Welcome Back - Account Unlocked";
                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <div style='text-align: center; margin-bottom: 20px;'>
                                <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 200px; height: auto;' />
                            </div>
                            <h2 style='color: #10b981;'>Welcome Back!</h2>
                            <p>Dear {userName},</p>
                            <p>Good news! Your Tunisia Motors account has been unlocked.</p>
                            
                            <div style='background: #d1fae5; padding: 20px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #10b981;'>
                                <p><strong>Account Status:</strong> Active</p>
                                <p><strong>Email:</strong> {user.Email}</p>
                                <p><strong>Date:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                            </div>

                            <p>You can now login to your account and access all features.</p>
                            <p>We're happy to have you back!</p>

                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='http://localhost:5271/account/login' style='background: #dc2626; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Login Now</a>
                            </div>

                            <p>Best regards,<br>Tunisia Motors Team</p>
                        </div>";

                    await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
                    
                    return Ok(new ApiResponse { Success = true, Message = "User unlocked successfully and welcome email sent" });
                }

                return BadRequest(new ApiResponse { Success = false, Message = "Failed to unlock user" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unlocking user: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error unlocking user" });
            }
        }

        // POST: api/admin/users/{userId}/send-verification
        [HttpPost("users/{userId}/send-verification")]
        public async Task<ActionResult<ApiResponse>> SendVerificationEmail(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });

                if (user.EmailConfirmed)
                    return BadRequest(new ApiResponse { Success = false, Message = "Email is already verified" });

                // Generate verification token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"http://localhost:5271/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // Send verification email
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);
                var userName = profile != null ? profile.FirstName : user.UserName;

                await _emailService.SendEmailConfirmationAsync(user.Email, userName, confirmationLink);
                
                _logger.LogInformation($"Verification email sent to user '{user.Email}'");
                return Ok(new ApiResponse { Success = true, Message = "Verification email sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending verification email: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error sending verification email" });
            }
        }

        // DELETE: api/admin/users/{userId}
        [HttpDelete("users/{userId}")]
        public async Task<ActionResult<ApiResponse>> DeleteUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User '{user.UserName}' deleted successfully");
                    return Ok(new ApiResponse { Success = true, Message = "User deleted successfully" });
                }

                return BadRequest(new ApiResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error deleting user" });
            }
        }

        // PUT: api/admin/roles/{roleId}
        [HttpPut("roles/{roleId}")]
        public async Task<ActionResult<ApiResponse>> UpdateRole(string roleId, [FromBody] UpdateRoleDto model)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Role not found" });

                role.Name = model.RoleName;
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role '{model.RoleName}' updated successfully");
                    return Ok(new ApiResponse { Success = true, Message = "Role updated successfully" });
                }

                return BadRequest(new ApiResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating role: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error updating role" });
            }
        }

        // GET: api/admin/roles/{roleId}/manage-users
        [HttpGet("roles/{roleId}/manage-users")]
        public async Task<ActionResult<ApiResponse<List<UserRoleManagementDto>>>> GetUsersForRoleManagement(string roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Role not found" });

                var allUsers = _userManager.Users.ToList();
                var userRoleDtos = new List<UserRoleManagementDto>();

                foreach (var user in allUsers)
                {
                    var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
                    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == user.Id);

                    userRoleDtos.Add(new UserRoleManagementDto
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = profile != null ? $"{profile.FirstName} {profile.LastName}" : "",
                        IsSelected = isInRole
                    });
                }

                return Ok(new ApiResponse<List<UserRoleManagementDto>>
                {
                    Success = true,
                    Data = userRoleDtos,
                    Message = $"{userRoleDtos.Count} users found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting users for role management: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving users" });
            }
        }

        // POST: api/admin/roles/{roleId}/manage-users
        [HttpPost("roles/{roleId}/manage-users")]
        public async Task<ActionResult<ApiResponse>> UpdateUsersInRole(string roleId, [FromBody] List<UserRoleManagementDto> model)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Role not found" });

                foreach (var userRole in model)
                {
                    var user = await _userManager.FindByIdAsync(userRole.UserId);
                    if (user == null) continue;

                    var isInRole = await _userManager.IsInRoleAsync(user, role.Name);

                    if (userRole.IsSelected && !isInRole)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                        _logger.LogInformation($"Added user '{user.UserName}' to role '{role.Name}'");
                    }
                    else if (!userRole.IsSelected && isInRole)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                        _logger.LogInformation($"Removed user '{user.UserName}' from role '{role.Name}'");
                    }
                }

                return Ok(new ApiResponse { Success = true, Message = "Users in role updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating users in role: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error updating users in role" });
            }
        }
    }
}
