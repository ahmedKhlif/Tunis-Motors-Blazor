using BlazorApp.Models;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public interface IPurchaseRequestService
    {
        Task<List<PurchaseRequestDto>> GetSellerPurchaseRequestsAsync(string sellerId);
        Task<PurchaseRequestDto> GetPurchaseRequestByIdAsync(int id);
        Task<bool> ResolvePurchaseRequestAsync(int id);
    }

    public class PurchaseRequestService : IPurchaseRequestService
    {
        private readonly IApiClient _apiClient;

        public PurchaseRequestService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<PurchaseRequestDto>> GetSellerPurchaseRequestsAsync(string sellerId)
        {
            try
            {
                var response = await _apiClient.GetAsync<List<PurchaseRequestDto>>($"api/purchaserequests/seller/{sellerId}");
                return response.Success && response.Data != null ? response.Data : new List<PurchaseRequestDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PurchaseRequestService] Error getting seller purchase requests: {ex.Message}");
                return new List<PurchaseRequestDto>();
            }
        }

        public async Task<PurchaseRequestDto> GetPurchaseRequestByIdAsync(int id)
        {
            try
            {
                var response = await _apiClient.GetAsync<PurchaseRequestDto>($"api/purchaserequests/{id}");
                return response.Success && response.Data != null ? response.Data : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PurchaseRequestService] Error getting purchase request: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ResolvePurchaseRequestAsync(int id)
        {
            try
            {
                var response = await _apiClient.PostAsync<bool>($"api/purchaserequests/{id}/resolve", null);
                return response.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PurchaseRequestService] Error resolving purchase request: {ex.Message}");
                return false;
            }
        }
    }
}
