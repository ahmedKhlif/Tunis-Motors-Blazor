using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public interface ICarRentalService
    {
        // CRUD Operations
        Task<CarRentalDto> GetRentalByIdAsync(int id);
        Task<PaginatedDto<CarRentalDto>> GetAllRentalsAsync(CarRentalFilterDto filter);
        Task<CarRentalDto> CreateRentalRequestAsync(string userId, CreateCarRentalDto model);
        Task<CarRentalDto> UpdateRentalAsync(int id, UpdateCarRentalDto model);

        // Status Management
        Task<CarRentalDto> ApproveRentalAsync(int id, string adminId, ApproveRentalDto model);
        Task<CarRentalDto> RejectRentalAsync(int id, string adminId, string reason);
        Task<CarRentalDto> ActivateRentalAsync(int id, string adminId);
        Task<CarRentalDto> ReturnRentalAsync(int id, string adminId, ReturnRentalDto model);
        Task<CarRentalDto> ExtendRentalAsync(int id, string adminId, ExtendRentalDto model);
        Task<CarRentalDto> CancelRentalAsync(int id, string userId, string reason);

        // Analytics & Statistics
        Task<RentalStatisticsDto> GetRentalStatisticsAsync();
        Task<List<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end);
        Task<List<CalendarEventDto>> GetUserCalendarEventsAsync(string userId, DateTime start, DateTime end);
        Task<List<CalendarEventDto>> GetSellerCalendarEventsAsync(string sellerId, DateTime start, DateTime end);

        // User-specific operations
        Task<PaginatedDto<CarRentalDto>> GetUserRentalsAsync(string userId, CarRentalFilterDto filter);
        Task<List<CarRentalDto>> GetSellerRentalsAsync(string sellerId);

        // Business Logic Helpers
        Task<bool> IsCarAvailableForRentalAsync(int carId, DateTime startDate, DateTime endDate, int? excludeRentalId = null);
        Task<decimal> CalculateLateFeesAsync(int rentalId);
        Task ProcessOverdueRentalsAsync();
    }
}