using BlazorApp.Models;

namespace BlazorApp.Services
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<PaginatedDto<OrderDto>> GetUserOrdersAsync(int page = 1, int pageSize = 10);
        Task<PaginatedDto<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto model);
        Task<OrderDto> CheckoutAsync(CheckoutDto model);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> CancelOrderAsync(int id, string reason = null);
        Task<bool> ApproveListingAsync(int listingId, ApproveListingDto model);
        Task<bool> RejectListingAsync(int listingId, RejectListingDto model);
        Task<PaginatedDto<OrderDto>> FilterOrdersAsync(string? status = null, string? search = null, int page = 1);
        Task<string> CreatePaymentIntentAsync(int orderId);
        Task<bool> ProcessPaymentAsync(int orderId, string paymentIntentId);
    }

    public class OrderService : IOrderService
    {
        private readonly IApiClient _apiClient;

        public OrderService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var response = await _apiClient.GetAsync<OrderDto>($"api/orders/{id}");
            return response.Data;
        }

        public async Task<PaginatedDto<OrderDto>> GetUserOrdersAsync(int page = 1, int pageSize = 10)
        {
            var response = await _apiClient.GetAsync<PaginatedDto<OrderDto>>(
                $"api/orders?page={page}&pageSize={pageSize}");
            return response.Data ?? new PaginatedDto<OrderDto>();
        }

        public async Task<PaginatedDto<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10)
        {
            var response = await _apiClient.GetAsync<PaginatedDto<OrderDto>>(
                $"api/orders/admin/all?page={page}&pageSize={pageSize}");
            return response.Data ?? new PaginatedDto<OrderDto>();
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto model)
        {
            var response = await _apiClient.PostAsync<OrderDto>("api/orders", model);
            return response.Data;
        }

        public async Task<OrderDto> CheckoutAsync(CheckoutDto model)
        {
            var response = await _apiClient.PostAsync<OrderDto>("api/orders/checkout", model);
            return response.Data;
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var updateDto = new UpdateOrderStatusDto { Status = status };
            var response = await _apiClient.PutAsync<object>($"api/orders/{id}/status", updateDto);
            return response.Success;
        }

        public async Task<bool> CancelOrderAsync(int id, string reason = null)
        {
            var cancelDto = new { Reason = reason };
            var response = await _apiClient.PostAsync<object>($"api/orders/{id}/cancel", cancelDto);
            return response.Success;
        }

        public async Task<bool> ApproveListingAsync(int listingId, ApproveListingDto model)
        {
            var response = await _apiClient.PostAsync<object>($"api/approval/{listingId}/approve", model);
            return response.Success;
        }

        public async Task<bool> RejectListingAsync(int listingId, RejectListingDto model)
        {
            var response = await _apiClient.PostAsync<object>($"api/approval/{listingId}/reject", model);
            return response.Success;
        }

        public async Task<PaginatedDto<OrderDto>> FilterOrdersAsync(string? status = null, string? search = null, int page = 1)
        {
            var response = await _apiClient.GetAsync<PaginatedDto<OrderDto>>(
                $"api/orders/admin/filter?status={status}&search={search}&page={page}");
            return response.Data ?? new PaginatedDto<OrderDto>();
        }

        public async Task<string> CreatePaymentIntentAsync(int orderId)
        {
            var response = await _apiClient.PostAsync<object>($"api/orders/{orderId}/create-payment-intent", null);
            
            if (!response.Success)
            {
                var errorMsg = response.Message ?? "Unknown error";
                if (response.Errors != null && response.Errors.Any())
                {
                    errorMsg += ": " + string.Join(", ", response.Errors);
                }
                throw new Exception(errorMsg);
            }
            
            if (response.Data == null)
            {
                throw new Exception("Payment intent created but no data returned from server");
            }
            
            // The Data is an object with clientSecret property (camelCase from API)
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(response.Data);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                
                // Try both camelCase and PascalCase property names
                if (doc.RootElement.TryGetProperty("clientSecret", out var clientSecretEl) || 
                    doc.RootElement.TryGetProperty("ClientSecret", out clientSecretEl))
                {
                    var clientSecret = clientSecretEl.GetString();
                    if (string.IsNullOrEmpty(clientSecret))
                    {
                        throw new Exception("Client secret is empty");
                    }
                    return clientSecret;
                }
                else
                {
                    // Debug: log the actual structure
                    var jsonString = doc.RootElement.GetRawText();
                    throw new Exception($"ClientSecret property not found in response. Available properties: {jsonString}");
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new Exception($"Failed to parse payment intent response: {ex.Message}");
            }
        }

        public async Task<bool> ProcessPaymentAsync(int orderId, string paymentIntentId)
        {
            var model = new { PaymentIntentId = paymentIntentId };
            var response = await _apiClient.PostAsync<object>($"api/orders/{orderId}/process-payment", model);
            return response.Success;
        }
    }
}
