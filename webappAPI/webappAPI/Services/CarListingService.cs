using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class CarListingService : ICarListingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CarListingService> _logger;

        public CarListingService(AppDbContext context, ILogger<CarListingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CarListingDto> GetCarListingByIdAsync(int id)
        {
            try
            {
                var listing = await _context.CarListings
                    .Include(c => c.Seller)
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.ProductId == id);

                if (listing == null)
                    return null;

                return MapToDto(listing);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting car listing: {ex.Message}");
                return null;
            }
        }

        public async Task<PaginatedDto<CarListingDto>> GetAllCarListingsAsync(CarListingFilterDto filter)
        {
            try
            {
                var query = _context.CarListings.AsQueryable();

                // Search by term
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                    query = query.Where(p => p.Name.Contains(filter.SearchTerm) ||
                                           p.Description.Contains(filter.SearchTerm) ||
                                           p.Brand.Contains(filter.SearchTerm));

                // Filter by category
                if (filter.CategoryId.HasValue)
                    query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

                // Filter by brand
                if (!string.IsNullOrEmpty(filter.Brand))
                    query = query.Where(p => p.Brand == filter.Brand);

                // Price range
                if (filter.MinPrice.HasValue)
                    query = query.Where(p => p.Price >= filter.MinPrice.Value);
                if (filter.MaxPrice.HasValue)
                    query = query.Where(p => p.Price <= filter.MaxPrice.Value);

                // Year range
                if (filter.MinYear.HasValue)
                    query = query.Where(p => p.Year >= filter.MinYear.Value);
                if (filter.MaxYear.HasValue)
                    query = query.Where(p => p.Year <= filter.MaxYear.Value);

                // Mileage
                if (filter.MaxMileage.HasValue)
                    query = query.Where(p => p.Mileage <= filter.MaxMileage.Value);

                // Fuel type
                if (!string.IsNullOrEmpty(filter.FuelType))
                    query = query.Where(p => p.FuelType == filter.FuelType);

                // Transmission
                if (!string.IsNullOrEmpty(filter.Transmission))
                    query = query.Where(p => p.Transmission == filter.Transmission);

                // Color
                if (!string.IsNullOrEmpty(filter.Color))
                    query = query.Where(p => p.Color == filter.Color);

                // Rating
                if (filter.MinRating.HasValue)
                    query = query.Where(p => p.Rating >= filter.MinRating.Value);

                // Only approved listings unless otherwise specified
                if (!filter.IncludeUnapproved)
                    query = query.Where(p => p.IsApproved);

                // Sorting
                query = filter.SortBy switch
                {
                    1 => query.OrderBy(p => p.Price),
                    2 => query.OrderByDescending(p => p.Price),
                    3 => query.OrderByDescending(p => p.Rating),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };

                var totalCount = await query.CountAsync();
                var items = await query
                    .Include(c => c.Seller)
                    .Include(c => c.Category)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return new PaginatedDto<CarListingDto>
                {
                    Data = items.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting car listings: {ex.Message}");
                return new PaginatedDto<CarListingDto>();
            }
        }

        public async Task<List<CarListingDto>> GetCarListingsByCategoryAsync(int categoryId)
        {
            try
            {
                var listings = await _context.CarListings
                    .Where(c => c.CategoryId == categoryId && c.IsApproved)
                    .Include(c => c.Seller)
                    .Include(c => c.Category)
                    .ToListAsync();

                return listings.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting listings by category: {ex.Message}");
                return new List<CarListingDto>();
            }
        }

        public async Task<List<CarListingDto>> GetUserListingsAsync(string sellerId)
        {
            try
            {
                var listings = await _context.CarListings
                    .Where(c => c.SellerId == sellerId)
                    .Include(c => c.Seller)
                    .Include(c => c.Category)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return listings.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user listings: {ex.Message}");
                return new List<CarListingDto>();
            }
        }

        public async Task<CarListingDto> CreateCarListingAsync(string sellerId, CreateCarListingDto model)
        {
            try
            {
                var listing = new CarListing
                {
                    Name = model.Name,
                    Price = model.Price,
                    Mileage = model.Mileage,
                    Year = model.Year,
                    Brand = model.Brand,
                    FuelType = model.FuelType,
                    Transmission = model.Transmission,
                    Color = model.Color,
                    VIN = model.VIN,
                    EngineSize = model.EngineSize,
                    Horsepower = model.Horsepower,
                    Doors = model.Doors,
                    Seats = model.Seats,
                    Description = model.Description,
                    Features = model.Features,
                    Condition = model.Condition,
                    Rating = model.Rating,
                    Image = model.Image,
                    AdditionalImages = model.AdditionalImages,
                    Location = model.Location,
                    QuantityInStock = model.QuantityInStock,
                    IsAvailableForRental = model.IsAvailableForRental,
                    RentalStock = model.RentalStock,
                    DailyRentalRate = model.DailyRentalRate,
                    CategoryId = model.CategoryId,
                    SellerId = sellerId,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CarListings.Add(listing);
                await _context.SaveChangesAsync();

                return await GetCarListingByIdAsync(listing.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating car listing: {ex.Message}");
                return null;
            }
        }

        public async Task<CarListingDto> UpdateCarListingAsync(int id, UpdateCarListingDto model)
        {
            try
            {
                var listing = await _context.CarListings.FindAsync(id);
                if (listing == null)
                    return null;

                if (!string.IsNullOrEmpty(model.Name)) listing.Name = model.Name;
                if (model.Price.HasValue) listing.Price = model.Price.Value;
                if (model.Mileage.HasValue) listing.Mileage = model.Mileage.Value;
                if (model.Year.HasValue) listing.Year = model.Year.Value;
                if (!string.IsNullOrEmpty(model.Brand)) listing.Brand = model.Brand;
                if (!string.IsNullOrEmpty(model.FuelType)) listing.FuelType = model.FuelType;
                if (!string.IsNullOrEmpty(model.Transmission)) listing.Transmission = model.Transmission;
                if (!string.IsNullOrEmpty(model.Color)) listing.Color = model.Color;
                if (!string.IsNullOrEmpty(model.VIN)) listing.VIN = model.VIN;
                if (model.EngineSize.HasValue) listing.EngineSize = model.EngineSize.Value;
                if (model.Horsepower.HasValue) listing.Horsepower = model.Horsepower.Value;
                if (model.Doors.HasValue) listing.Doors = model.Doors.Value;
                if (model.Seats.HasValue) listing.Seats = model.Seats.Value;
                if (!string.IsNullOrEmpty(model.Description)) listing.Description = model.Description;
                if (!string.IsNullOrEmpty(model.Features)) listing.Features = model.Features;
                if (!string.IsNullOrEmpty(model.Condition)) listing.Condition = model.Condition;
                if (model.Rating.HasValue) listing.Rating = model.Rating.Value;
                if (!string.IsNullOrEmpty(model.Image)) listing.Image = model.Image;
                if (!string.IsNullOrEmpty(model.AdditionalImages)) listing.AdditionalImages = model.AdditionalImages;
                if (!string.IsNullOrEmpty(model.Location)) listing.Location = model.Location;
                if (model.QuantityInStock.HasValue) listing.QuantityInStock = model.QuantityInStock.Value;
                if (model.IsAvailableForRental.HasValue) listing.IsAvailableForRental = model.IsAvailableForRental.Value;
                if (model.RentalStock.HasValue) listing.RentalStock = model.RentalStock.Value;
                if (model.DailyRentalRate.HasValue) listing.DailyRentalRate = model.DailyRentalRate.Value;
                if (model.CategoryId.HasValue) listing.CategoryId = model.CategoryId.Value;

                listing.UpdatedAt = DateTime.UtcNow;
                _context.CarListings.Update(listing);
                await _context.SaveChangesAsync();

                return await GetCarListingByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating car listing: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteCarListingAsync(int id)
        {
            try
            {
                var listing = await _context.CarListings.FindAsync(id);
                if (listing == null)
                    return false;

                _context.CarListings.Remove(listing);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting car listing: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ApproveCarListingAsync(int id, string adminId, string? note)
        {
            try
            {
                var listing = await _context.CarListings.FindAsync(id);
                if (listing == null)
                    return false;

                listing.IsApproved = true;
                listing.ApprovedAt = DateTime.UtcNow;
                listing.ApprovedBy = adminId;
                listing.AdminApprovalNote = note;

                _context.CarListings.Update(listing);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving car listing: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RejectCarListingAsync(int id, string? note)
        {
            try
            {
                var listing = await _context.CarListings.FindAsync(id);
                if (listing == null)
                    return false;

                listing.AdminApprovalNote = note ?? "Listing rejected";
                _context.CarListings.Remove(listing);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting car listing: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CarListingDto>> GetPendingApprovalsAsync()
        {
            try
            {
                var listings = await _context.CarListings
                    .Where(c => !c.IsApproved)
                    .Include(c => c.Seller)
                    .Include(c => c.Category)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return listings.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting pending approvals: {ex.Message}");
                return new List<CarListingDto>();
            }
        }

        public async Task<List<string>> GetBrandsAsync()
        {
            try
            {
                return await _context.CarListings
                    .Select(c => c.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting brands: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetFuelTypesAsync()
        {
            try
            {
                return await _context.CarListings
                    .Select(c => c.FuelType)
                    .Distinct()
                    .OrderBy(f => f)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting fuel types: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetTransmissionsAsync()
        {
            try
            {
                return await _context.CarListings
                    .Select(c => c.Transmission)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transmissions: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetColorsAsync()
        {
            try
            {
                return await _context.CarListings
                    .Select(c => c.Color)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting colors: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> IncrementViewsAsync(int id)
        {
            try
            {
                var listing = await _context.CarListings.FindAsync(id);
                if (listing == null)
                    return false;

                listing.Views++;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error incrementing views for listing {id}: {ex.Message}");
                return false;
            }
        }

        private CarListingDto MapToDto(CarListing listing)
        {
            return new CarListingDto
            {
                ProductId = listing.ProductId,
                Name = listing.Name,
                Price = listing.Price,
                Mileage = listing.Mileage,
                Year = listing.Year,
                Brand = listing.Brand,
                FuelType = listing.FuelType,
                Transmission = listing.Transmission,
                Color = listing.Color,
                VIN = listing.VIN,
                EngineSize = listing.EngineSize,
                Horsepower = listing.Horsepower,
                Doors = listing.Doors,
                Seats = listing.Seats,
                Description = listing.Description,
                Features = listing.Features,
                Condition = listing.Condition,
                Rating = listing.Rating,
                IsApproved = listing.IsApproved,
                AdminApprovalNote = listing.AdminApprovalNote,
                ApprovedAt = listing.ApprovedAt,
                ApprovedBy = listing.ApprovedBy,
                Views = listing.Views,
                Image = listing.Image,
                AdditionalImages = listing.AdditionalImages,
                Location = listing.Location,
                QuantityInStock = listing.QuantityInStock,
                IsAvailableForRental = listing.IsAvailableForRental,
                RentalStock = listing.RentalStock,
                DailyRentalRate = listing.DailyRentalRate,
                CategoryId = listing.CategoryId,
                SellerId = listing.SellerId,
                CreatedAt = listing.CreatedAt,
                UpdatedAt = listing.UpdatedAt
            };
        }
    }
}
