using BlazorApp.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorApp.Services
{
    public interface ICompareService
    {
        Task<List<CarListingDto>> GetComparedCarsAsync();
        Task<bool> AddToCompareAsync(CarListingDto car);
        Task<bool> RemoveFromCompareAsync(int carId);
        Task<bool> IsInCompareAsync(int carId);
        Task ClearCompareAsync();
        Task<int> GetCompareCountAsync();
        event Action OnCompareChanged;
    }

    public class CompareService : ICompareService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CompareStorageKey = "compare_cars";
        private const int MaxCompareItems = 4; // Limit to 4 cars for comparison

        public event Action OnCompareChanged;

        public CompareService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<List<CarListingDto>> GetComparedCarsAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CompareStorageKey);
                if (string.IsNullOrEmpty(json))
                    return new List<CarListingDto>();

                return JsonSerializer.Deserialize<List<CarListingDto>>(json) ?? new List<CarListingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting compared cars: {ex.Message}");
                return new List<CarListingDto>();
            }
        }

        public async Task<bool> AddToCompareAsync(CarListingDto car)
        {
            try
            {
                var comparedCars = await GetComparedCarsAsync();

                // Check if already in compare
                if (comparedCars.Any(c => c.Id == car.Id))
                    return false;

                // Check max limit
                if (comparedCars.Count >= MaxCompareItems)
                {
                    // Remove oldest item
                    comparedCars.RemoveAt(0);
                }

                comparedCars.Add(car);
                var json = JsonSerializer.Serialize(comparedCars);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CompareStorageKey, json);

                OnCompareChanged?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to compare: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveFromCompareAsync(int carId)
        {
            try
            {
                var comparedCars = await GetComparedCarsAsync();
                var removed = comparedCars.RemoveAll(c => c.Id == carId);

                if (removed > 0)
                {
                    var json = JsonSerializer.Serialize(comparedCars);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CompareStorageKey, json);
                    OnCompareChanged?.Invoke();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from compare: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsInCompareAsync(int carId)
        {
            try
            {
                var comparedCars = await GetComparedCarsAsync();
                return comparedCars.Any(c => c.Id == carId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking compare: {ex.Message}");
                return false;
            }
        }

        public async Task ClearCompareAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", CompareStorageKey);
                OnCompareChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing compare: {ex.Message}");
            }
        }

        public async Task<int> GetCompareCountAsync()
        {
            try
            {
                var comparedCars = await GetComparedCarsAsync();
                return comparedCars.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting compare count: {ex.Message}");
                return 0;
            }
        }
    }
}

