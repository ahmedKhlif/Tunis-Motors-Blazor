using System.ComponentModel.DataAnnotations;

namespace webappAPI.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalListings { get; set; }
        public int ApprovedListings { get; set; }
        public int TotalOrders { get; set; }
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }
        public int PendingApprovals { get; set; }
        public List<RecentListingDto> RecentListings { get; set; } = new();
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<PendingMessageDto> PendingMessages { get; set; } = new();
    }

    public class DashboardAnalyticsDto
    {
        public decimal MonthlyRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<BrandStatsDto> PopularBrands { get; set; } = new();
        public List<CategoryStatsDto> ListingsByCategory { get; set; } = new();
        public List<MonthlyRegistrationDto> UserRegistrations { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenueData { get; set; } = new();
    }

    public class RecentListingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsApproved { get; set; }
        public decimal Price { get; set; }
        public string SellerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string BuyerId { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }

    public class PendingMessageDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class BrandStatsDto
    {
        public string Brand { get; set; }
        public int Count { get; set; }
    }

    public class CategoryStatsDto
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }

    public class MonthlyRegistrationDto
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
    }

    public class PendingListingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string SellerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AdminApprovalNote { get; set; }
    }
}
