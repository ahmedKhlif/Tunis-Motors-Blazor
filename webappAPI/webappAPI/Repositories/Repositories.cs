using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.Models;

namespace webappAPI.Repositories
{
    public class CarListingRepository : ICarListingRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CarListingRepository> _logger;

        public CarListingRepository(AppDbContext context, ILogger<CarListingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CarListing> GetByIdAsync(int id)
        {
            return await _context.CarListings
                .Include(c => c.Seller)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.ProductId == id);
        }

        public async Task<List<CarListing>> GetAllAsync()
        {
            return await _context.CarListings
                .Include(c => c.Seller)
                .Include(c => c.Category)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CarListing>> GetApprovedAsync()
        {
            return await _context.CarListings
                .Include(c => c.Seller)
                .Include(c => c.Category)
                .Where(c => c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CarListing>> GetPendingApprovalAsync()
        {
            return await _context.CarListings
                .Include(c => c.Seller)
                .Include(c => c.Category)
                .Where(c => !c.IsApproved)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CarListing>> GetBySellerIdAsync(string sellerId)
        {
            return await _context.CarListings
                .Include(c => c.Seller)
                .Include(c => c.Category)
                .Where(c => c.SellerId == sellerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CarListing>> GetByCategoryAsync(int categoryId)
        {
            return await _context.CarListings
                .Include(c => c.Seller)
                .Include(c => c.Category)
                .Where(c => c.CategoryId == categoryId && c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CarListing>> FilterAsync(string searchTerm, int? categoryId, string brand,
            decimal? minPrice, decimal? maxPrice, int? minYear, int? maxYear, int? maxMileage,
            string fuelType, string transmission, string color, bool includeUnapproved = false)
        {
            var query = _context.CarListings
                .Include(c => c.Seller)
                .Include(c => c.Category)
                .AsQueryable();

            // Apply approval filter
            if (!includeUnapproved)
                query = query.Where(c => c.IsApproved);

            // Search term filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    c.Description.ToLower().Contains(search) ||
                    c.Brand.ToLower().Contains(search));
            }

            // Category filter
            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            // Brand filter
            if (!string.IsNullOrEmpty(brand))
                query = query.Where(c => c.Brand == brand);

            // Price filters
            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            // Year filters
            if (minYear.HasValue)
                query = query.Where(c => c.Year >= minYear.Value);

            if (maxYear.HasValue)
                query = query.Where(c => c.Year <= maxYear.Value);

            // Mileage filter
            if (maxMileage.HasValue)
                query = query.Where(c => c.Mileage <= maxMileage.Value);

            // Fuel type filter
            if (!string.IsNullOrEmpty(fuelType))
                query = query.Where(c => c.FuelType == fuelType);

            // Transmission filter
            if (!string.IsNullOrEmpty(transmission))
                query = query.Where(c => c.Transmission == transmission);

            // Color filter
            if (!string.IsNullOrEmpty(color))
                query = query.Where(c => c.Color == color);

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<List<CarListing>> GetByBrandAsync(string brand)
        {
            return await _context.CarListings
                .Include(c => c.Seller)
                .Include(c => c.Category)
                .Where(c => c.Brand == brand && c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<CarListing> CreateAsync(CarListing listing)
        {
            _context.CarListings.Add(listing);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Car listing created: {listing.ProductId}");
            return listing;
        }

        public async Task<CarListing> UpdateAsync(CarListing listing)
        {
            listing.UpdatedAt = DateTime.UtcNow;
            _context.CarListings.Update(listing);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Car listing updated: {listing.ProductId}");
            return listing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var listing = await _context.CarListings.FindAsync(id);
            if (listing == null)
                return false;

            _context.CarListings.Remove(listing);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Car listing deleted: {id}");
            return true;
        }

        public async Task<bool> ApproveAsync(int id, string approvedBy, string? approvalNote)
        {
            var listing = await _context.CarListings.FindAsync(id);
            if (listing == null)
                return false;

            listing.IsApproved = true;
            listing.ApprovedBy = approvedBy;
            listing.ApprovedAt = DateTime.UtcNow;
            listing.AdminApprovalNote = approvalNote;
            listing.UpdatedAt = DateTime.UtcNow;

            _context.CarListings.Update(listing);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Car listing approved: {id}");
            return true;
        }

        public async Task<bool> RejectAsync(int id, string rejectionReason)
        {
            var listing = await _context.CarListings.FindAsync(id);
            if (listing == null)
                return false;

            listing.IsApproved = false;
            listing.AdminApprovalNote = rejectionReason;
            listing.UpdatedAt = DateTime.UtcNow;

            _context.CarListings.Update(listing);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Car listing rejected: {id}");
            return true;
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.CarListings.CountAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _context.CarListings.Where(c => !c.IsApproved).CountAsync();
        }
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.CarListing)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.CarListing)
                .Where(o => o.UserId == userId && !string.IsNullOrEmpty(o.CustomerName))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.CarListing)
                .Where(o => !string.IsNullOrEmpty(o.CustomerName))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Order created: {order.Id}");
            return order;
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Order updated: {order.Id}");
            return order;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Order deleted: {id}");
            return true;
        }

        public async Task<Order> GetCartByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.CarListing)
                .FirstOrDefaultAsync(o => o.UserId == userId && string.IsNullOrEmpty(o.CustomerName));
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.CarListing)
                .Where(o => o.Status == status && !string.IsNullOrEmpty(o.CustomerName))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.CustomerName))
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<decimal> GetMonthlyRevenueAsync(int month, int year)
        {
            return await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.CustomerName) &&
                           o.OrderDate.Month == month &&
                           o.OrderDate.Year == year)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            return await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.CustomerName))
                .CountAsync();
        }
    }

    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderItemRepository> _logger;

        public OrderItemRepository(AppDbContext context, ILogger<OrderItemRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderItem> GetByIdAsync(int id)
        {
            return await _context.OrderItems
                .Include(oi => oi.CarListing)
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.CarListing)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<OrderItem> CreateAsync(OrderItem item)
        {
            _context.OrderItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<OrderItem> UpdateAsync(OrderItem item)
        {
            _context.OrderItems.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item == null)
                return false;

            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(AppDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Category created: {category.CategoryId}");
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Category updated: {category.CategoryId}");
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Category deleted: {id}");
            return true;
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == name);
        }

        public async Task<int> GetListingCountAsync(int categoryId)
        {
            return await _context.CarListings
                .Where(c => c.CategoryId == categoryId)
                .CountAsync();
        }
    }

    public class WishlistRepository : IWishlistRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WishlistRepository> _logger;

        public WishlistRepository(AppDbContext context, ILogger<WishlistRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Wishlist> GetByIdAsync(int id)
        {
            return await _context.Wishlists
                .Include(w => w.CarListing)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<List<Wishlist>> GetByUserIdAsync(string userId)
        {
            return await _context.Wishlists
                .Include(w => w.CarListing)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<Wishlist> GetWishlistItemAsync(string userId, int carListingId)
        {
            return await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == carListingId);
        }

        public async Task<Wishlist> CreateAsync(Wishlist wishlist)
        {
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Item added to wishlist: {wishlist.Id}");
            return wishlist;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist == null)
                return false;

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Item removed from wishlist: {id}");
            return true;
        }

        public async Task<bool> RemoveAsync(string userId, int carListingId)
        {
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == carListingId);

            if (wishlist == null)
                return false;

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCountAsync(string userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .CountAsync();
        }

        public async Task<bool> IsInWishlistAsync(string userId, int carListingId)
        {
            return await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == carListingId);
        }
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(AppDbContext context, ILogger<MessageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Message> GetByIdAsync(int id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Message>> GetUserMessagesAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<List<Message>> GetUnreadMessagesAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<Message> CreateAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Message created: {message.Id}");
            return message;
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return false;

            message.IsRead = true;
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Message>> GetConversationAsync(string userId, string otherUserId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }

    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserProfileRepository> _logger;

        public UserProfileRepository(AppDbContext context, ILogger<UserProfileRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserProfile> GetByUserIdAsync(string userId)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId);
        }

        public async Task<UserProfile> CreateAsync(UserProfile profile)
        {
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User profile created: {profile.Id}");
            return profile;
        }

        public async Task<UserProfile> UpdateAsync(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User profile updated: {profile.Id}");
            return profile;
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId);
            if (profile == null)
                return false;

            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
