using BlazorApp.Models;

namespace BlazorApp.Services
{
    public interface ICarRentalService
    {
        Task<ApiResponse<CarRentalDto>> CreateRentalRequestAsync(CreateCarRentalDto request);
        Task<ApiResponse<PaginatedDto<CarRentalDto>>> GetMyRentalsAsync(CarRentalFilterDto filter);
        Task<ApiResponse<PaginatedDto<CarRentalDto>>> GetAllRentalsAsync(CarRentalFilterDto filter);
        Task<ApiResponse<CarRentalDto>> GetRentalByIdAsync(int rentalId);
        Task<ApiResponse<CarRentalDto>> CancelRentalAsync(int rentalId, string reason);
        Task<ApiResponse<CarRentalDto>> ReturnRentalAsync(int rentalId, ReturnRentalDto returnData);
        Task<ApiResponse<CarRentalDto>> ApproveRentalAsync(int rentalId, ApproveRentalDto approvalData);
        Task<ApiResponse<CarRentalDto>> RejectRentalAsync(int rentalId, string reason);
        Task<ApiResponse<CarRentalDto>> ActivateRentalAsync(int rentalId);
        Task<ApiResponse<CarRentalDto>> ExtendRentalAsync(int rentalId, ExtendRentalDto extensionData);
        Task<ApiResponse<RentalStatisticsDto>> GetRentalStatisticsAsync();
        Task<ApiResponse<List<CalendarEventDto>>> GetRentalCalendarEventsAsync(DateTime start, DateTime end);
        Task<ApiResponse<List<CalendarEventDto>>> GetAdminCalendarEventsAsync(DateTime start, DateTime end);
        Task<ApiResponse<List<CalendarEventDto>>> GetSellerCalendarEventsAsync(DateTime start, DateTime end);
        Task<ApiResponse<List<CarRentalDto>>> GetSellerRentalsAsync(string sellerId);
    }
}