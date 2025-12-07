using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public interface ICarListingService
    {
        Task<CarListingDto> GetCarListingByIdAsync(int id);
        Task<PaginatedDto<CarListingDto>> GetAllCarListingsAsync(CarListingFilterDto filter);
        Task<List<CarListingDto>> GetCarListingsByCategoryAsync(int categoryId);
        Task<List<CarListingDto>> GetUserListingsAsync(string sellerId);
        Task<CarListingDto> CreateCarListingAsync(string sellerId, CreateCarListingDto model);
        Task<CarListingDto> UpdateCarListingAsync(int id, UpdateCarListingDto model);
        Task<bool> DeleteCarListingAsync(int id);
        Task<bool> ApproveCarListingAsync(int id, string adminId, string? note);
        Task<bool> RejectCarListingAsync(int id, string? note);
        Task<List<CarListingDto>> GetPendingApprovalsAsync();
        Task<List<string>> GetBrandsAsync();
        Task<List<string>> GetFuelTypesAsync();
        Task<List<string>> GetTransmissionsAsync();
        Task<List<string>> GetColorsAsync();
        Task<bool> IncrementViewsAsync(int id);
    }

    public interface ICategoryService
    {
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto model);
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto model);
        Task<bool> DeleteCategoryAsync(int id);
    }

    public interface IOrderService
    {
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<PaginatedDto<OrderDto>> GetUserOrdersAsync(string userId, int page = 1, int pageSize = 10);
        Task<PaginatedDto<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10);
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto model);
        Task<OrderDto> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto model, string updatedBy);
        Task<bool> CancelOrderAsync(int id);
        Task<PaginatedDto<OrderDto>> FilterOrdersAsync(string? status, string? search, int page = 1, int pageSize = 10);
    }

    public interface IWishlistService
    {
        Task<List<WishlistDto>> GetUserWishlistAsync(string userId);
        Task<WishlistDto> AddToWishlistAsync(string userId, int productId);
        Task<bool> RemoveFromWishlistAsync(string userId, int productId);
        Task<bool> IsInWishlistAsync(string userId, int productId);
    }

    public interface IMessageService
    {
        Task<MessageDto> GetMessageByIdAsync(int id);
        Task<List<MessageDto>> GetInboxAsync(string userId);
        Task<List<MessageDto>> GetSentMessagesAsync(string userId);
        Task<MessageDto> SendMessageAsync(string senderId, CreateMessageDto model);
        Task<bool> MarkAsReadAsync(int messageId);
        Task<bool> DeleteMessageAsync(int id);
    }

    public interface IPurchaseRequestService
    {
        Task<PurchaseRequestDto> GetPurchaseRequestByIdAsync(int id);
        Task<List<PurchaseRequestDto>> GetUserPurchaseRequestsAsync(string userId);
        Task<List<PurchaseRequestDto>> GetSellerPurchaseRequestsAsync(string sellerId);
        Task<PurchaseRequestDto> CreatePurchaseRequestAsync(string customerId, CreatePurchaseRequestDto model);
        Task<PurchaseRequestDto> UpdatePurchaseRequestAsync(int id, UpdatePurchaseRequestDto model);
        Task<bool> ClosePurchaseRequestAsync(int id);
    }
}
