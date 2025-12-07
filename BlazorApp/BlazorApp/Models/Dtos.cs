namespace BlazorApp.Models
{
    // Auth Models
    public class LoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class RegisterModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string Role { get; set; } = "Buyer";
    }

    // Shared DTOs
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<string>? Roles { get; set; }
        public UserProfileDto? Profile { get; set; }
    }

    public class UserProfileDto
    {
        public string Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateUserProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
    }

    public class UserWithRoleDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    // Category Models
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string CategoryName { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    // Car Listing Models
    public class CarListingDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Mileage { get; set; }
        public int Year { get; set; }
        public string Brand { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public string Color { get; set; }
        public string? VIN { get; set; }
        public decimal? EngineSize { get; set; }
        public int? Horsepower { get; set; }
        public int? Doors { get; set; }
        public int? Seats { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public string Condition { get; set; }
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public string? AdminApprovalNote { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public int Views { get; set; }
        public string Image { get; set; }
        public string? AdditionalImages { get; set; }
        public string? Location { get; set; }
        public int QuantityInStock { get; set; }
        public bool IsAvailableForRental { get; set; }
        public int RentalStock { get; set; }
        public decimal? DailyRentalRate { get; set; }
        public int? CategoryId { get; set; }
        public string? SellerId { get; set; }
        public UserDto? Seller { get; set; }
        public CategoryDto? Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CarListingFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public int? MaxMileage { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public string? Color { get; set; }
        public int? MinRating { get; set; }
        public int SortBy { get; set; } = 0;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public bool IncludeUnapproved { get; set; } = false;
    }

    public class CreateCarListingDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Mileage { get; set; }
        public int Year { get; set; }
        public string Brand { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public string Color { get; set; }
        public string? VIN { get; set; }
        public decimal? EngineSize { get; set; }
        public int? Horsepower { get; set; }
        public int? Doors { get; set; }
        public int? Seats { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public string Condition { get; set; }
        public int Rating { get; set; } = 5;
        public string? Image { get; set; }
        public string? AdditionalImages { get; set; } // JSON array of image paths
        public string? Location { get; set; }
        public bool IsAvailableForRental { get; set; } = false;
        public int RentalStock { get; set; } = 0;
        public decimal? DailyRentalRate { get; set; }
        public int? CategoryId { get; set; }
        public int QuantityInStock { get; set; } = 1;
    }

    public class UpdateCarListingDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Mileage { get; set; }
        public int? Year { get; set; }
        public string? Brand { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public string? Color { get; set; }
        public string? VIN { get; set; }
        public decimal? EngineSize { get; set; }
        public int? Horsepower { get; set; }
        public int? Doors { get; set; }
        public int? Seats { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public string? Condition { get; set; }
        public int? Rating { get; set; }
        public string? Image { get; set; }
        public string? AdditionalImages { get; set; }
        public string? Location { get; set; }
        public bool? IsAvailableForRental { get; set; }
        public int? RentalStock { get; set; }
        public decimal? DailyRentalRate { get; set; }
        public int? CategoryId { get; set; }
        public int? QuantityInStock { get; set; }
    }

    // Order Models
    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public string? StatusUpdatedBy { get; set; }
        public string UserId { get; set; }
        public List<OrderItemDto>? Items { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public CarListingDto? Product { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateOrderDto
    {
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public List<CreateOrderItemDto>? Items { get; set; }
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; }
    }

    public class CheckoutDto
    {
        public string StripeTokenId { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class ApproveListingDto
    {
        public string ApprovalNote { get; set; }
    }

    public class RejectListingDto
    {
        public string RejectionReason { get; set; }
    }

    public class FileUploadResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }

    // Wishlist Models
    public class WishlistDto
    {
        public int WishlistId { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public CarListingDto? CarListing { get; set; }
        public DateTime AddedAt { get; set; }
    }

    // Message Models
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public int? ListingId { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class CreateMessageDto
    {
        public string ReceiverId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int? ListingId { get; set; }
    }

    // Pagination
    public class PaginatedDto<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // API Response
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
    }

    // Cart Models
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public CarListingDto Product { get; set; }
    }

    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    // Dashboard Models
    public class DashboardStatsDto
    {
        public int TotalListings { get; set; }
        public int PendingApprovals { get; set; }
        public int ApprovedListings { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }
        public List<RecentListingDto> RecentListings { get; set; } = new List<RecentListingDto>();
        public List<RecentOrderDto> RecentOrders { get; set; } = new List<RecentOrderDto>();
        public List<PendingMessageDto> PendingMessages { get; set; } = new List<PendingMessageDto>();
    }

    public class DashboardAnalyticsDto
    {
        public decimal MonthlyRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<BrandStatsDto> PopularBrands { get; set; } = new List<BrandStatsDto>();
        public List<CategoryStatsDto> ListingsByCategory { get; set; } = new List<CategoryStatsDto>();
        public List<MonthlyRegistrationDto> UserRegistrations { get; set; } = new List<MonthlyRegistrationDto>();
        public List<MonthlyRevenueDto> MonthlyRevenueData { get; set; } = new List<MonthlyRevenueDto>();
    }

    public class RecentListingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string SellerName { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string BuyerId { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
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

    // Admin Models
    public class UserManagementDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime? CreatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsLocked { get; set; }
        public int ListingCount { get; set; }
    }

    public class RoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UserCount { get; set; }
        public List<string> Users { get; set; } = new List<string>();
    }

    public class RoleDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UserCount { get; set; }
        public List<UserManagementDto> Users { get; set; } = new List<UserManagementDto>();
    }

    public class CreateRoleDto
    {
        public string RoleName { get; set; }
        public string Description { get; set; }
    }

    public class UpdateRoleDto
    {
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

    // Purchase Request Models
    public class PurchaseRequestDto
    {
        public int Id { get; set; }
        public int CarListingId { get; set; }
        public string CarListingName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Message { get; set; }
        public decimal CarListingPrice { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Car Rental DTOs
    public enum RentalStatus
    {
        REQUESTED,
        APPROVED,
        ACTIVE,
        OVERDUE,
        RETURNED,
        CANCELLED
    }

    public class CarRentalDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CarName { get; set; }
        public string CarImage { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public RentalStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public int ExtendedCount { get; set; }
        public string? Notes { get; set; }
        public decimal? DailyRate { get; set; }
        public int? InitialMileage { get; set; }
        public int? ReturnMileage { get; set; }
        public string? DamageNotes { get; set; }
        public decimal? LateFees { get; set; }

        // Computed properties
        public string FormattedRequestedDate => RequestedAt.ToString("MMM dd, yyyy HH:mm");
        public string FormattedApprovedDate => ApprovedAt?.ToString("MMM dd, yyyy HH:mm") ?? "Not approved";
        public string FormattedPickupDate => PickupDate?.ToString("MMM dd, yyyy HH:mm") ?? "Not picked up";
        public string FormattedReturnDueDate => ReturnDueDate?.ToString("MMM dd, yyyy") ?? "Not set";
        public string FormattedReturnedDate => ReturnedAt?.ToString("MMM dd, yyyy HH:mm") ?? "Not returned";
        public int RemainingDays { get; set; }
        public int OverdueDays { get; set; }
        public bool IsOverdue { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class CreateCarRentalDto
    {
        public int CarId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDueDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ApproveRentalDto
    {
        public decimal DailyRate { get; set; }
        public string? Notes { get; set; }
    }

    public class ReturnRentalDto
    {
        public DateTime ReturnDate { get; set; }
        public int? ReturnMileage { get; set; }
        public string? DamageNotes { get; set; }
        public decimal? LateFees { get; set; } = 0;
    }

    public class ExtendRentalDto
    {
        public int AdditionalDays { get; set; }
        public string? Reason { get; set; }
    }

    public class RentalStatisticsDto
    {
        public int TotalRentals { get; set; }
        public int PendingRequests { get; set; }
        public int ActiveRentals { get; set; }
        public int OverdueRentals { get; set; }
        public int ReturnedToday { get; set; }
        public int ReturnedThisWeek { get; set; }
        public int ReturnedThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public double AverageRentalDuration { get; set; }
    }

    public class CarRentalFilterDto
    {
        public RentalStatus? Status { get; set; }
        public string? UserId { get; set; }
        public int? CarId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsOverdue { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Color { get; set; }
        public string Status { get; set; }
        public string CarName { get; set; }
        public string UserName { get; set; }
        public bool IsOverdue { get; set; }
    }
}
