using System.ComponentModel.DataAnnotations;

namespace webappAPI.DTOs
{
    public class UserManagementDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsLocked { get; set; }
        public List<string> Roles { get; set; } = new();
        public string CreatedAt { get; set; }
        public int ListingsCount { get; set; }
        public int OrdersCount { get; set; }
    }

    public class CreateRoleDto
    {
        [Required]
        [StringLength(256)]
        public string RoleName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class AssignRoleDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string RoleName { get; set; }
    }

    public class RemoveRoleDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string RoleName { get; set; }
    }

    public class LockUserDto
    {
        [Required]
        public string UserId { get; set; }

        public DateTime? LockoutEndDate { get; set; }
    }

    public class UnlockUserDto
    {
        [Required]
        public string UserId { get; set; }
    }

    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int ActiveSellers { get; set; }
        public int TotalListings { get; set; }
        public int PendingApprovals { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<UserManagementDto> RecentUsers { get; set; } = new();
    }

    public class UpdateRoleDto
    {
        [Required]
        [StringLength(256)]
        public string RoleName { get; set; }
    }

    public class UserRoleManagementDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsSelected { get; set; }
    }
}
