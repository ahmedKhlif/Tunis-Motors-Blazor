using BlazorApp.Models;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public class CarRentalService : ICarRentalService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5237/api/carrentals";

        public CarRentalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<CarRentalDto>> CreateRentalRequestAsync(CreateCarRentalDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_baseUrl, request);
                return await response.Content.ReadFromJsonAsync<ApiResponse<CarRentalDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = $"Error creating rental request: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<PaginatedDto<CarRentalDto>>> GetMyRentalsAsync(CarRentalFilterDto filter)
        {
            try
            {
                var queryString = BuildQueryString(filter);
                var response = await _httpClient.GetAsync($"{_baseUrl}/my{queryString}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedDto<CarRentalDto>>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<PaginatedDto<CarRentalDto>>
                {
                    Success = false,
                    Message = $"Error loading rentals: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<CarRentalDto>> GetRentalByIdAsync(int rentalId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/{rentalId}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<CarRentalDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = $"Error loading rental: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<CarRentalDto>> CancelRentalAsync(int rentalId, string reason)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{rentalId}/cancel", reason);
                return await response.Content.ReadFromJsonAsync<ApiResponse<CarRentalDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = $"Error cancelling rental: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<CarRentalDto>> ReturnRentalAsync(int rentalId, ReturnRentalDto returnData)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{rentalId}/return", returnData);
                return await response.Content.ReadFromJsonAsync<ApiResponse<CarRentalDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = $"Error returning rental: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<CarRentalDto>> ApproveRentalAsync(int rentalId, ApproveRentalDto approvalData)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{rentalId}/approve", approvalData);
                return await response.Content.ReadFromJsonAsync<ApiResponse<CarRentalDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = $"Error approving rental: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<CarRentalDto>> ActivateRentalAsync(int rentalId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/{rentalId}/activate", null);
                return await response.Content.ReadFromJsonAsync<ApiResponse<CarRentalDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = $"Error activating rental: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<CarRentalDto>> ExtendRentalAsync(int rentalId, ExtendRentalDto extensionData)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{rentalId}/extend", extensionData);
                return await response.Content.ReadFromJsonAsync<ApiResponse<CarRentalDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = $"Error extending rental: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<RentalStatisticsDto>> GetRentalStatisticsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/statistics");
                return await response.Content.ReadFromJsonAsync<ApiResponse<RentalStatisticsDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<RentalStatisticsDto>
                {
                    Success = false,
                    Message = $"Error loading statistics: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<CalendarEventDto>>> GetRentalCalendarEventsAsync(DateTime start, DateTime end)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/my-calendar?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<CalendarEventDto>>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CalendarEventDto>>
                {
                    Success = false,
                    Message = $"Error loading calendar events: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<CalendarEventDto>>> GetSellerCalendarEventsAsync(DateTime start, DateTime end)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/seller-calendar?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<CalendarEventDto>>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CalendarEventDto>>
                {
                    Success = false,
                    Message = $"Error loading seller calendar events: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<PaginatedDto<CarRentalDto>>> GetAllRentalsAsync(CarRentalFilterDto filter)
        {
            try
            {
                var queryString = BuildQueryString(filter);
                var response = await _httpClient.GetAsync($"{_baseUrl}{queryString}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedDto<CarRentalDto>>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<PaginatedDto<CarRentalDto>>
                {
                    Success = false,
                    Message = $"Error loading all rentals: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<CarRentalDto>> RejectRentalAsync(int rentalId, string reason)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{rentalId}/reject", reason);
                return await response.Content.ReadFromJsonAsync<ApiResponse<CarRentalDto>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = $"Error rejecting rental: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<CalendarEventDto>>> GetAdminCalendarEventsAsync(DateTime start, DateTime end)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/calendar?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<CalendarEventDto>>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CalendarEventDto>>
                {
                    Success = false,
                    Message = $"Error loading calendar events: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<CarRentalDto>>> GetSellerRentalsAsync(string sellerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/seller/{sellerId}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<CarRentalDto>>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CarRentalDto>>
                {
                    Success = false,
                    Message = $"Error loading seller rentals: {ex.Message}"
                };
            }
        }

        private string BuildQueryString(CarRentalFilterDto filter)
        {
            var parameters = new List<string>();

            if (filter.Status.HasValue)
                parameters.Add($"status={filter.Status}");
            if (filter.UserId != null)
                parameters.Add($"userId={filter.UserId}");
            if (filter.CarId.HasValue)
                parameters.Add($"carId={filter.CarId}");
            if (filter.StartDate.HasValue)
                parameters.Add($"startDate={filter.StartDate.Value:yyyy-MM-dd}");
            if (filter.EndDate.HasValue)
                parameters.Add($"endDate={filter.EndDate.Value:yyyy-MM-dd}");
            if (filter.IsOverdue.HasValue)
                parameters.Add($"isOverdue={filter.IsOverdue.Value}");

            parameters.Add($"page={filter.Page}");
            parameters.Add($"pageSize={filter.PageSize}");

            return parameters.Any() ? "?" + string.Join("&", parameters) : "";
        }
    }
}