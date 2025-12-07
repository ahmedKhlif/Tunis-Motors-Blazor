using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webappAPI.DTOs;
using webappAPI.Repositories;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class ApprovalController : ControllerBase
    {
        private readonly ICarListingRepository _carListingRepository;
        private readonly ILogger<ApprovalController> _logger;

        public ApprovalController(ICarListingRepository carListingRepository, ILogger<ApprovalController> logger)
        {
            _carListingRepository = carListingRepository;
            _logger = logger;
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<List<CarListingDto>>>> GetPendingListings()
        {
            try
            {
                var pendingListings = await _carListingRepository.GetPendingApprovalAsync();
                return Ok(new ApiResponse<List<CarListingDto>>
                {
                    Success = true,
                    Data = pendingListings.Select(p => new CarListingDto
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Price = p.Price,
                        Brand = p.Brand,
                        Year = p.Year,
                        Mileage = p.Mileage,
                        FuelType = p.FuelType,
                        Transmission = p.Transmission,
                        Color = p.Color,
                        VIN = p.VIN,
                        EngineSize = p.EngineSize,
                        Horsepower = p.Horsepower,
                        Doors = p.Doors,
                        Seats = p.Seats,
                        Description = p.Description,
                        Features = p.Features,
                        Condition = p.Condition,
                        Rating = p.Rating,
                        IsApproved = p.IsApproved,
                        AdminApprovalNote = p.AdminApprovalNote,
                        ApprovedAt = p.ApprovedAt,
                        ApprovedBy = p.ApprovedBy,
                        Image = p.Image,
                        AdditionalImages = p.AdditionalImages,
                        Location = p.Location,
                        QuantityInStock = p.QuantityInStock,
                        CategoryId = p.CategoryId,
                        SellerId = p.SellerId,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }).ToList(),
                    Message = "Pending listings retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving pending listings: {ex.Message}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving pending listings"
                });
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult<ApiResponse>> ApproveCarListing(int id, [FromBody] ApproveListingDto model)
        {
            try
            {
                var listing = await _carListingRepository.GetByIdAsync(id);
                if (listing == null)
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Listing not found"
                    });

                if (listing.IsApproved)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Listing is already approved"
                    });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _carListingRepository.ApproveAsync(id, userId, model.AdminNote);

                if (!result)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to approve listing"
                    });

                _logger.LogInformation($"Listing {id} approved by user {userId}");

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Listing approved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving listing {id}: {ex.Message}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while approving the listing"
                });
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult<ApiResponse>> RejectCarListing(int id, [FromBody] RejectListingDto model)
        {
            try
            {
                var listing = await _carListingRepository.GetByIdAsync(id);
                if (listing == null)
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Listing not found"
                    });

                if (listing.IsApproved)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Cannot reject an already approved listing"
                    });

                var result = await _carListingRepository.RejectAsync(id, model.RejectionReason);

                if (!result)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to reject listing"
                    });

                _logger.LogInformation($"Listing {id} rejected");

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Listing rejected successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting listing {id}: {ex.Message}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while rejecting the listing"
                });
            }
        }

        [HttpGet("listing/{id}")]
        public async Task<ActionResult<ApiResponse<CarListingDto>>> GetListingForApproval(int id)
        {
            try
            {
                var listing = await _carListingRepository.GetByIdAsync(id);
                if (listing == null)
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Listing not found"
                    });

                var dto = new CarListingDto
                {
                    ProductId = listing.ProductId,
                    Name = listing.Name,
                    Price = listing.Price,
                    Brand = listing.Brand,
                    Year = listing.Year,
                    Mileage = listing.Mileage,
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
                    Image = listing.Image,
                    AdditionalImages = listing.AdditionalImages,
                    Location = listing.Location,
                    QuantityInStock = listing.QuantityInStock,
                    CategoryId = listing.CategoryId,
                    SellerId = listing.SellerId,
                    CreatedAt = listing.CreatedAt,
                    UpdatedAt = listing.UpdatedAt
                };

                return Ok(new ApiResponse<CarListingDto>
                {
                    Success = true,
                    Data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving listing {id}: {ex.Message}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving the listing"
                });
            }
        }
    }
}
