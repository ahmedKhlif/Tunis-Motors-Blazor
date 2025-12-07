using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class PurchaseRequestService : IPurchaseRequestService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PurchaseRequestService> _logger;

        public PurchaseRequestService(AppDbContext context, ILogger<PurchaseRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PurchaseRequestDto> GetPurchaseRequestByIdAsync(int id)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests
                    .Include(pr => pr.Customer)
                    .Include(pr => pr.CarListing)
                    .FirstOrDefaultAsync(pr => pr.PurchaseRequestId == id);

                return purchaseRequest != null ? MapToDto(purchaseRequest) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting purchase request: {ex.Message}");
                return null;
            }
        }

        public async Task<List<PurchaseRequestDto>> GetUserPurchaseRequestsAsync(string userId)
        {
            try
            {
                var requests = await _context.PurchaseRequests
                    .Where(pr => pr.CustomerId == userId)
                    .Include(pr => pr.Customer)
                    .Include(pr => pr.CarListing)
                    .OrderByDescending(pr => pr.CreatedAt)
                    .ToListAsync();

                return requests.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user purchase requests: {ex.Message}");
                return new List<PurchaseRequestDto>();
            }
        }

        public async Task<List<PurchaseRequestDto>> GetSellerPurchaseRequestsAsync(string sellerId)
        {
            try
            {
                var requests = await _context.PurchaseRequests
                    .Where(pr => pr.CarListing.SellerId == sellerId)
                    .Include(pr => pr.Customer)
                    .Include(pr => pr.CarListing)
                    .OrderByDescending(pr => pr.CreatedAt)
                    .ToListAsync();

                return requests.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting seller purchase requests: {ex.Message}");
                return new List<PurchaseRequestDto>();
            }
        }

        public async Task<PurchaseRequestDto> CreatePurchaseRequestAsync(string customerId, CreatePurchaseRequestDto model)
        {
            try
            {
                var purchaseRequest = new PurchaseRequest
                {
                    CustomerId = customerId,
                    ProductId = model.ProductId,
                    Message = model.Message,
                    PhoneNumber = model.PhoneNumber,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.PurchaseRequests.Add(purchaseRequest);
                await _context.SaveChangesAsync();

                return await GetPurchaseRequestByIdAsync(purchaseRequest.PurchaseRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating purchase request: {ex.Message}");
                return null;
            }
        }

        public async Task<PurchaseRequestDto> UpdatePurchaseRequestAsync(int id, UpdatePurchaseRequestDto model)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests.FindAsync(id);
                if (purchaseRequest == null)
                    return null;

                if (!string.IsNullOrEmpty(model.SellerResponse))
                {
                    purchaseRequest.SellerResponse = model.SellerResponse;
                    purchaseRequest.RespondedAt = DateTime.UtcNow;
                    purchaseRequest.Status = "Responded";
                }

                if (!string.IsNullOrEmpty(model.Status))
                    purchaseRequest.Status = model.Status;

                _context.PurchaseRequests.Update(purchaseRequest);
                await _context.SaveChangesAsync();

                return await GetPurchaseRequestByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating purchase request: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ClosePurchaseRequestAsync(int id)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests.FindAsync(id);
                if (purchaseRequest == null)
                    return false;

                purchaseRequest.Status = "Closed";
                _context.PurchaseRequests.Update(purchaseRequest);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error closing purchase request: {ex.Message}");
                return false;
            }
        }

        private PurchaseRequestDto MapToDto(PurchaseRequest purchaseRequest)
        {
            return new PurchaseRequestDto
            {
                PurchaseRequestId = purchaseRequest.PurchaseRequestId,
                CustomerId = purchaseRequest.CustomerId,
                ProductId = purchaseRequest.ProductId,
                Message = purchaseRequest.Message,
                PhoneNumber = purchaseRequest.PhoneNumber,
                Status = purchaseRequest.Status,
                SellerResponse = purchaseRequest.SellerResponse,
                CreatedAt = purchaseRequest.CreatedAt,
                RespondedAt = purchaseRequest.RespondedAt
            };
        }
    }
}
