using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/dashboard/stats
        [HttpGet("stats")]
        public ActionResult<ApiResponse<DashboardStatsDto>> GetStats()
        {
            try
            {
                var totalListings = _context.CarListings.Count();
                var pendingApprovals = _context.CarListings.Count(c => !c.IsApproved);
                var approvedListings = _context.CarListings.Count(c => c.IsApproved);
                var totalUsers = _context.Users.Count();
                var totalOrders = _context.Orders.Count();
                var totalMessages = _context.Messages.Count();
                var unreadMessages = _context.Messages.Count(m => !m.IsRead);

                // Get recent listings
                var recentListings = _context.CarListings
                    .Include(c => c.Seller)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .Select(c => new RecentListingDto
                    {
                        Id = c.ProductId,
                        Name = c.Name,
                        Price = c.Price,
                        SellerName = c.Seller != null ? c.Seller.UserName : "Unknown",
                        IsApproved = c.IsApproved,
                        CreatedAt = c.CreatedAt
                    })
                    .ToList();

                // Get recent orders
                var recentOrders = _context.Orders
                    .Include(o => o.Items)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new RecentOrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.Id.ToString(),
                        BuyerId = o.UserId,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status.ToString(),
                        ItemCount = o.Items.Count,
                        OrderDate = o.OrderDate
                    })
                    .ToList();

                // Get pending messages
                var pendingMessages = _context.Messages
                    .Where(m => !m.IsRead)
                    .Include(m => m.Sender)
                    .OrderByDescending(m => m.SentAt)
                    .Take(5)
                    .Select(m => new PendingMessageDto
                    {
                        Id = m.Id,
                        SenderName = m.Sender != null ? m.Sender.UserName : "Unknown",
                        Subject = m.Subject,
                        SentAt = m.SentAt
                    })
                    .ToList();

                var stats = new DashboardStatsDto
                {
                    TotalListings = totalListings,
                    PendingApprovals = pendingApprovals,
                    ApprovedListings = approvedListings,
                    TotalUsers = totalUsers,
                    TotalOrders = totalOrders,
                    TotalMessages = totalMessages,
                    UnreadMessages = unreadMessages,
                    RecentListings = recentListings,
                    RecentOrders = recentOrders,
                    PendingMessages = pendingMessages
                };

                return Ok(new ApiResponse<DashboardStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Dashboard stats retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting dashboard stats: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving dashboard stats" });
            }
        }

        // GET: api/dashboard/analytics
        [HttpGet("analytics")]
        public ActionResult<ApiResponse<DashboardAnalyticsDto>> GetAnalytics()
        {
            try
            {
                // Calculate monthly revenue
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var monthlyRevenue = _context.Orders
                    .Where(o => o.OrderDate.Month == currentMonth && o.OrderDate.Year == currentYear)
                    .Sum(o => o.TotalAmount);

                // Calculate total revenue
                var totalRevenue = _context.Orders.Sum(o => o.TotalAmount);

                // Get popular brands
                var popularBrands = _context.CarListings
                    .Where(c => !string.IsNullOrEmpty(c.Brand) && c.IsApproved)
                    .GroupBy(c => c.Brand)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => new BrandStatsDto
                    {
                        Brand = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                // Get listings by category
                var listingsByCategory = _context.CarListings
                    .Where(c => c.IsApproved && c.Category != null)
                    .Include(c => c.Category)
                    .GroupBy(c => c.Category!.CategoryName)
                    .Select(g => new CategoryStatsDto
                    {
                        Category = g.Key,
                        Count = g.Count()
                    })
                    .ToList();
                
                // Add uncategorized count if any
                var uncategorizedCount = _context.CarListings
                    .Count(c => c.IsApproved && c.Category == null);
                
                if (uncategorizedCount > 0)
                {
                    listingsByCategory.Add(new CategoryStatsDto
                    {
                        Category = "Uncategorized",
                        Count = uncategorizedCount
                    });
                }

                // Get user registrations by month (last 12 months)
                var userRegistrations = new List<MonthlyRegistrationDto>();
                for (int i = 11; i >= 0; i--)
                {
                    var targetDate = DateTime.Now.AddMonths(-i);
                    var count = _context.Users
                        .Where(u => u.Id != null) // Placeholder - modify if UserCreatedAt is added
                        .Count();
                    
                    userRegistrations.Add(new MonthlyRegistrationDto
                    {
                        Month = targetDate.ToString("MMM yyyy"),
                        Count = count
                    });
                }

                // Get monthly revenue data (last 12 months)
                var monthlyRevenueData = new List<MonthlyRevenueDto>();
                for (int i = 11; i >= 0; i--)
                {
                    var targetDate = DateTime.Now.AddMonths(-i);
                    var revenue = _context.Orders
                        .Where(o => o.OrderDate.Year == targetDate.Year && o.OrderDate.Month == targetDate.Month)
                        .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                    
                    monthlyRevenueData.Add(new MonthlyRevenueDto
                    {
                        Month = targetDate.ToString("MMM"),
                        Revenue = revenue
                    });
                }

                var analytics = new DashboardAnalyticsDto
                {
                    MonthlyRevenue = monthlyRevenue,
                    TotalRevenue = totalRevenue,
                    PopularBrands = popularBrands,
                    ListingsByCategory = listingsByCategory,
                    UserRegistrations = userRegistrations,
                    MonthlyRevenueData = monthlyRevenueData
                };

                return Ok(new ApiResponse<DashboardAnalyticsDto>
                {
                    Success = true,
                    Data = analytics,
                    Message = "Analytics data retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting analytics: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving analytics" });
            }
        }

        // GET: api/dashboard/pending-listings
        [HttpGet("pending-listings")]
        public ActionResult<ApiResponse<List<PendingListingDto>>> GetPendingListings()
        {
            try
            {
                var pending = _context.CarListings
                    .Where(c => !c.IsApproved)
                    .Include(c => c.Seller)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new PendingListingDto
                    {
                        Id = c.ProductId,
                        Name = c.Name,
                        Brand = c.Brand,
                        Price = c.Price,
                        SellerName = c.Seller != null ? c.Seller.UserName : "Unknown",
                        CreatedAt = c.CreatedAt,
                        AdminApprovalNote = c.AdminApprovalNote
                    })
                    .ToList();

                return Ok(new ApiResponse<List<PendingListingDto>>
                {
                    Success = true,
                    Data = pending,
                    Message = $"{pending.Count} pending listings found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting pending listings: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving pending listings" });
            }
        }
    }
}
