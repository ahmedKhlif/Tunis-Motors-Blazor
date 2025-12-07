using webappAPI.Models;

namespace webappAPI.Repositories
{
    public interface ICarListingRepository
    {
        Task<CarListing> GetByIdAsync(int id);
        Task<List<CarListing>> GetAllAsync();
        Task<List<CarListing>> GetApprovedAsync();
        Task<List<CarListing>> GetPendingApprovalAsync();
        Task<List<CarListing>> GetBySellerIdAsync(string sellerId);
        Task<List<CarListing>> GetByCategoryAsync(int categoryId);
        Task<List<CarListing>> FilterAsync(string searchTerm, int? categoryId, string brand, 
            decimal? minPrice, decimal? maxPrice, int? minYear, int? maxYear, int? maxMileage,
            string fuelType, string transmission, string color, bool includeUnapproved = false);
        Task<List<CarListing>> GetByBrandAsync(string brand);
        Task<CarListing> CreateAsync(CarListing listing);
        Task<CarListing> UpdateAsync(CarListing listing);
        Task<bool> DeleteAsync(int id);
        Task<bool> ApproveAsync(int id, string approvedBy, string? approvalNote);
        Task<bool> RejectAsync(int id, string rejectionReason);
        Task<int> GetCountAsync();
        Task<int> GetPendingCountAsync();
    }

    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task<List<Order>> GetAllAsync();
        Task<Order> CreateAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int id);
        Task<Order> GetCartByUserIdAsync(string userId);
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetMonthlyRevenueAsync(int month, int year);
        Task<int> GetTotalOrdersAsync();
    }

    public interface IOrderItemRepository
    {
        Task<OrderItem> GetByIdAsync(int id);
        Task<List<OrderItem>> GetByOrderIdAsync(int orderId);
        Task<OrderItem> CreateAsync(OrderItem item);
        Task<OrderItem> UpdateAsync(OrderItem item);
        Task<bool> DeleteAsync(int id);
    }

    public interface ICategoryRepository
    {
        Task<Category> GetByIdAsync(int id);
        Task<List<Category>> GetAllAsync();
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task<Category> GetByNameAsync(string name);
        Task<int> GetListingCountAsync(int categoryId);
    }

    public interface IWishlistRepository
    {
        Task<Wishlist> GetByIdAsync(int id);
        Task<List<Wishlist>> GetByUserIdAsync(string userId);
        Task<Wishlist> GetWishlistItemAsync(string userId, int carListingId);
        Task<Wishlist> CreateAsync(Wishlist wishlist);
        Task<bool> DeleteAsync(int id);
        Task<bool> RemoveAsync(string userId, int carListingId);
        Task<int> GetCountAsync(string userId);
        Task<bool> IsInWishlistAsync(string userId, int carListingId);
    }

    public interface IMessageRepository
    {
        Task<Message> GetByIdAsync(int id);
        Task<List<Message>> GetUserMessagesAsync(string userId);
        Task<List<Message>> GetUnreadMessagesAsync(string userId);
        Task<Message> CreateAsync(Message message);
        Task<Message> UpdateAsync(Message message);
        Task<bool> DeleteAsync(int id);
        Task<bool> MarkAsReadAsync(int id);
        Task<List<Message>> GetConversationAsync(string userId, string otherUserId);
    }

    public interface IUserProfileRepository
    {
        Task<UserProfile> GetByUserIdAsync(string userId);
        Task<UserProfile> CreateAsync(UserProfile profile);
        Task<UserProfile> UpdateAsync(UserProfile profile);
        Task<bool> DeleteAsync(string userId);
    }
}
