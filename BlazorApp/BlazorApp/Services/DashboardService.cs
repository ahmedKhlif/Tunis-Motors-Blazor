using BlazorApp.Models;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
        Task<DashboardAnalyticsDto> GetAnalyticsAsync();
        Task<List<CarListingDto>> GetPendingListingsAsync();
    }

    public class DashboardService : IDashboardService
    {
        private readonly IApiClient _apiClient;

        public DashboardService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            try
            {
                Console.WriteLine("[DashboardService] Getting stats from API...");
                var response = await _apiClient.GetAsync<DashboardStatsDto>("api/dashboard/stats");
                
                if (response.Success && response.Data != null)
                {
                    Console.WriteLine($"[DashboardService] Stats loaded: {response.Data.TotalListings} listings, {response.Data.TotalUsers} users");
                    return response.Data;
                }
                
                Console.WriteLine($"[DashboardService] Failed to load stats: {response.Message}");
                return new DashboardStatsDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardService] Error getting dashboard stats: {ex.Message}");
                return new DashboardStatsDto();
            }
        }

        public async Task<DashboardAnalyticsDto> GetAnalyticsAsync()
        {
            try
            {
                Console.WriteLine("[DashboardService] Getting analytics from API...");
                var response = await _apiClient.GetAsync<DashboardAnalyticsDto>("api/dashboard/analytics");
                
                if (response.Success && response.Data != null)
                {
                    Console.WriteLine($"[DashboardService] Analytics loaded: {response.Data.PopularBrands?.Count ?? 0} brands, {response.Data.ListingsByCategory?.Count ?? 0} categories");
                    return response.Data;
                }
                
                Console.WriteLine($"[DashboardService] Failed to load analytics: {response.Message}");
                return new DashboardAnalyticsDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardService] Error getting dashboard analytics: {ex.Message}");
                return new DashboardAnalyticsDto();
            }
        }

        public async Task<List<CarListingDto>> GetPendingListingsAsync()
        {
            try
            {
                Console.WriteLine("[DashboardService] Getting pending listings from API...");
                var response = await _apiClient.GetAsync<List<CarListingDto>>("api/dashboard/pending-listings");
                
                if (response.Success && response.Data != null)
                {
                    Console.WriteLine($"[DashboardService] Pending listings loaded: {response.Data.Count}");
                    return response.Data;
                }
                
                Console.WriteLine($"[DashboardService] Failed to load pending listings: {response.Message}");
                return new List<CarListingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardService] Error getting pending listings: {ex.Message}");
                return new List<CarListingDto>();
            }
        }
    }
}