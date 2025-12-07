using BlazorApp.Models;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public interface IApprovalService
    {
        Task<List<CarListingDto>> GetPendingListingsAsync();
        Task<List<CarListingDto>> GetApprovedListingsAsync();
        Task<ApiResponse> ApproveListingAsync(int listingId, string adminNote = null);
        Task<ApiResponse> RejectListingAsync(int listingId, string rejectionReason);
        Task<CarListingDto> GetListingForApprovalAsync(int listingId);
        Task<PendingListingDto> GetPendingListingByIdAsync(int listingId);
    }

    public class ApprovalService : IApprovalService
    {
        private readonly IApiClient _apiClient;

        public ApprovalService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<CarListingDto>> GetPendingListingsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<CarListingDto>>("api/approval/pending");
                return response?.Data ?? new List<CarListingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending listings: {ex.Message}");
                return new List<CarListingDto>();
            }
        }

        public async Task<List<CarListingDto>> GetApprovedListingsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<CarListingDto>>("api/approval/approved");
                return response?.Data ?? new List<CarListingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting approved listings: {ex.Message}");
                return new List<CarListingDto>();
            }
        }

        public async Task<ApiResponse> ApproveListingAsync(int listingId, string adminNote = null)
        {
            try
            {
                var model = new { AdminNote = adminNote };
                var response = await _apiClient.PostAsync<object>($"api/approval/{listingId}/approve", model);
                return new ApiResponse { Success = response.Success, Message = response.Message };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving listing: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while approving the listing" };
            }
        }

        public async Task<ApiResponse> RejectListingAsync(int listingId, string rejectionReason)
        {
            try
            {
                var model = new { RejectionReason = rejectionReason };
                var response = await _apiClient.PostAsync<object>($"api/approval/{listingId}/reject", model);
                return new ApiResponse { Success = response.Success, Message = response.Message };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rejecting listing: {ex.Message}");
                return new ApiResponse { Success = false, Message = "An error occurred while rejecting the listing" };
            }
        }

        public async Task<CarListingDto> GetListingForApprovalAsync(int listingId)
        {
            try
            {
                var response = await _apiClient.GetAsync<CarListingDto>($"api/approval/listing/{listingId}");
                return response?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting listing for approval: {ex.Message}");
                return null;
            }
        }

        public async Task<PendingListingDto> GetPendingListingByIdAsync(int listingId)
        {
            try
            {
                var response = await _apiClient.GetAsync<PendingListingDto>($"api/approval/pending/{listingId}");
                return response?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending listing {listingId}: {ex.Message}");
                return null;
            }
        }
    }
}