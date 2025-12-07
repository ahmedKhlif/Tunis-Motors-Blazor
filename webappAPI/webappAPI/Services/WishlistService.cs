using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WishlistService> _logger;

        public WishlistService(AppDbContext context, ILogger<WishlistService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<WishlistDto>> GetUserWishlistAsync(string userId)
        {
            try
            {
                var wishlistItems = await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Include(w => w.CarListing)
                    .ThenInclude(c => c.Category)
                    .OrderByDescending(w => w.AddedAt)
                    .ToListAsync();

                return wishlistItems.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user wishlist: {ex.Message}");
                return new List<WishlistDto>();
            }
        }

        public async Task<WishlistDto> AddToWishlistAsync(string userId, int productId)
        {
            try
            {
                var existingItem = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

                if (existingItem != null)
                    return MapToDto(existingItem);

                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedAt = DateTime.UtcNow
                };

                _context.Wishlists.Add(wishlistItem);
                await _context.SaveChangesAsync();
                // Load the related CarListing including Category for richer DTO mapping
                var loadedListing = await _context.CarListings
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.ProductId == productId);
                if (loadedListing != null)
                {
                    wishlistItem.CarListing = loadedListing;
                }

                return MapToDto(wishlistItem);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to wishlist: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RemoveFromWishlistAsync(string userId, int productId)
        {
            try
            {
                var wishlistItem = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

                if (wishlistItem == null)
                    return false;

                _context.Wishlists.Remove(wishlistItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing from wishlist: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsInWishlistAsync(string userId, int productId)
        {
            try
            {
                return await _context.Wishlists
                    .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking wishlist: {ex.Message}");
                return false;
            }
        }

        private WishlistDto MapToDto(Wishlist wishlist)
        {
            return new WishlistDto
            {
                Id = wishlist.Id,
                UserId = wishlist.UserId,
                ProductId = wishlist.ProductId,
                CarListing = wishlist.CarListing != null ? new CarListingDto
                {
                    ProductId = wishlist.CarListing.ProductId,
                    Name = wishlist.CarListing.Name,
                    Price = wishlist.CarListing.Price,
                    Mileage = wishlist.CarListing.Mileage,
                    Year = wishlist.CarListing.Year,
                    Brand = wishlist.CarListing.Brand,
                    FuelType = wishlist.CarListing.FuelType,
                    Transmission = wishlist.CarListing.Transmission,
                    Color = wishlist.CarListing.Color,
                    VIN = wishlist.CarListing.VIN,
                    EngineSize = wishlist.CarListing.EngineSize,
                    Horsepower = wishlist.CarListing.Horsepower,
                    Doors = wishlist.CarListing.Doors,
                    Seats = wishlist.CarListing.Seats,
                    Description = wishlist.CarListing.Description,
                    Features = wishlist.CarListing.Features,
                    Condition = wishlist.CarListing.Condition,
                    Rating = wishlist.CarListing.Rating,
                    IsApproved = wishlist.CarListing.IsApproved,
                    AdminApprovalNote = wishlist.CarListing.AdminApprovalNote,
                    ApprovedAt = wishlist.CarListing.ApprovedAt,
                    ApprovedBy = wishlist.CarListing.ApprovedBy,
                    Views = wishlist.CarListing.Views,
                    Image = wishlist.CarListing.Image,
                    AdditionalImages = wishlist.CarListing.AdditionalImages,
                    Location = wishlist.CarListing.Location,
                    QuantityInStock = wishlist.CarListing.QuantityInStock,
                    CategoryId = wishlist.CarListing.CategoryId,
                    SellerId = wishlist.CarListing.SellerId,
                    CreatedAt = wishlist.CarListing.CreatedAt,
                    UpdatedAt = wishlist.CarListing.UpdatedAt,
                    Category = wishlist.CarListing.Category != null ? new CategoryDto
                    {
                        CategoryId = wishlist.CarListing.Category.CategoryId,
                        CategoryName = wishlist.CarListing.Category.CategoryName,
                        Image = wishlist.CarListing.Category.Image,
                        CreatedAt = wishlist.CarListing.Category.CreatedAt,
                        UpdatedAt = wishlist.CarListing.Category.UpdatedAt
                    } : null
                } : null,
                AddedAt = wishlist.AddedAt
            };
        }
    }
}
