using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;
using webappAPI.Services;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CarRentalsController : ControllerBase
    {
        private readonly ICarRentalService _rentalService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CarRentalsController> _logger;
        private readonly AppDbContext _context;

        public CarRentalsController(
            ICarRentalService rentalService,
            UserManager<IdentityUser> userManager,
            ILogger<CarRentalsController> logger,
            AppDbContext context)
        {
            _rentalService = rentalService;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        private async Task<string> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? throw new UnauthorizedAccessException("User not found");
        }

        private async Task<bool> IsAdminOrManager()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return false;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains("Admin") || roles.Contains("Manager");
        }

        // GET: api/carrentals
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<PaginatedDto<CarRentalDto>>>> GetAllRentals(
            [FromQuery] RentalStatus? status,
            [FromQuery] string? userId,
            [FromQuery] int? carId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool? isOverdue,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                var filter = new CarRentalFilterDto
                {
                    Status = status,
                    UserId = userId,
                    CarId = carId,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsOverdue = isOverdue,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _rentalService.GetAllRentalsAsync(filter);
                return Ok(new ApiResponse<PaginatedDto<CarRentalDto>> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rentals");
                return BadRequest(new ApiResponse<PaginatedDto<CarRentalDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve rentals"
                });
            }
        }

        // GET: api/carrentals/my
        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<PaginatedDto<CarRentalDto>>>> GetMyRentals(
            [FromQuery] RentalStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var filter = new CarRentalFilterDto
                {
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _rentalService.GetUserRentalsAsync(userId, filter);
                return Ok(new ApiResponse<PaginatedDto<CarRentalDto>> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user rentals");
                return BadRequest(new ApiResponse<PaginatedDto<CarRentalDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve your rentals"
                });
            }
        }

        // GET: api/carrentals/seller/{sellerId}
        [HttpGet("seller/{sellerId}")]
        [Authorize(Roles = "Seller,Admin,Manager")]
        public async Task<ActionResult<ApiResponse<List<CarRentalDto>>>> GetSellerRentals(string sellerId)
        {
            try
            {
                var currentUserId = await GetCurrentUserId();
                var isAdmin = await IsAdminOrManager();

                // Sellers can only view their own rentals, admins can view any
                if (!isAdmin && currentUserId != sellerId)
                {
                    return Forbid();
                }

                var rentals = await _rentalService.GetSellerRentalsAsync(sellerId);
                return Ok(new ApiResponse<List<CarRentalDto>> 
                { 
                    Success = true, 
                    Data = rentals 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seller rentals for seller {SellerId}", sellerId);
                return BadRequest(new ApiResponse<List<CarRentalDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve seller rentals"
                });
            }
        }

        // GET: api/carrentals/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CarRentalDto>>> GetRental(int id)
        {
            try
            {
                var rental = await _rentalService.GetRentalByIdAsync(id);
                if (rental == null)
                    return NotFound(new ApiResponse<CarRentalDto>
                    {
                        Success = false,
                        Message = "Rental not found"
                    });

                // Check if user can view this rental
                var userId = await GetCurrentUserId();
                var isAdmin = await IsAdminOrManager();
                
                // Check if user is a seller who owns the car
                var user = await _userManager.GetUserAsync(User);
                var isSeller = false;
                var isSellerOwner = false;
                
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    isSeller = roles.Contains("Seller");
                }
                
                if (isSeller && rental.CarId > 0)
                {
                    // Check if seller owns the car in this rental
                    var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ProductId == rental.CarId);
                    if (car != null && car.SellerId == userId)
                    {
                        isSellerOwner = true;
                    }
                }

                // Allow access if: admin/manager, rental owner, or seller who owns the car
                if (!isAdmin && rental.UserId != userId && !isSellerOwner)
                    return Forbid();

                return Ok(new ApiResponse<CarRentalDto> { Success = true, Data = rental });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rental {Id}", id);
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Failed to retrieve rental"
                });
            }
        }

        // POST: api/carrentals
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CarRentalDto>>> CreateRental(CreateCarRentalDto model)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var rental = await _rentalService.CreateRentalRequestAsync(userId, model);
                return CreatedAtAction(nameof(GetRental),
                    new { id = rental.Id },
                    new ApiResponse<CarRentalDto> { Success = true, Data = rental });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rental");
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Failed to create rental request"
                });
            }
        }

        // POST: api/carrentals/{id}/approve
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,Manager,Seller")]
        public async Task<ActionResult<ApiResponse<CarRentalDto>>> ApproveRental(int id, ApproveRentalDto model)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var isAdmin = await IsAdminOrManager();
                
                // Check if seller owns the car
                if (!isAdmin)
                {
                    var rental = await _rentalService.GetRentalByIdAsync(id);
                    if (rental == null)
                        return NotFound(new ApiResponse<CarRentalDto> { Success = false, Message = "Rental not found" });
                    
                    var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ProductId == rental.CarId);
                    if (car == null || car.SellerId != userId)
                        return Forbid();
                }
                
                var adminId = userId;
                var rentalResult = await _rentalService.ApproveRentalAsync(id, adminId, model);
                return Ok(new ApiResponse<CarRentalDto>
                {
                    Success = true,
                    Data = rentalResult,
                    Message = "Rental approved successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Rental not found"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving rental {Id}", id);
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Failed to approve rental"
                });
            }
        }

        // POST: api/carrentals/{id}/reject
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,Manager,Seller")]
        public async Task<ActionResult<ApiResponse<CarRentalDto>>> RejectRental(int id, [FromBody] string reason)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var isAdmin = await IsAdminOrManager();
                
                // Check if seller owns the car
                if (!isAdmin)
                {
                    var rental = await _rentalService.GetRentalByIdAsync(id);
                    if (rental == null)
                        return NotFound(new ApiResponse<CarRentalDto> { Success = false, Message = "Rental not found" });
                    
                    var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ProductId == rental.CarId);
                    if (car == null || car.SellerId != userId)
                        return Forbid();
                }
                
                var adminId = userId;
                var rentalResult = await _rentalService.RejectRentalAsync(id, adminId, reason);
                return Ok(new ApiResponse<CarRentalDto>
                {
                    Success = true,
                    Data = rentalResult,
                    Message = "Rental rejected successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Rental not found"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting rental {Id}", id);
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Failed to reject rental"
                });
            }
        }

        // POST: api/carrentals/{id}/activate
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin,Manager,Seller")]
        public async Task<ActionResult<ApiResponse<CarRentalDto>>> ActivateRental(int id)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var isAdmin = await IsAdminOrManager();
                
                // Check if seller owns the car
                if (!isAdmin)
                {
                    var rental = await _rentalService.GetRentalByIdAsync(id);
                    if (rental == null)
                        return NotFound(new ApiResponse<CarRentalDto> { Success = false, Message = "Rental not found" });
                    
                    var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ProductId == rental.CarId);
                    if (car == null || car.SellerId != userId)
                        return Forbid();
                }
                
                var adminId = userId;
                var rentalResult = await _rentalService.ActivateRentalAsync(id, adminId);
                return Ok(new ApiResponse<CarRentalDto>
                {
                    Success = true,
                    Data = rentalResult,
                    Message = "Rental activated successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Rental not found"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating rental {Id}", id);
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Failed to activate rental"
                });
            }
        }

        // POST: api/carrentals/{id}/return
        [HttpPost("{id}/return")]
        [Authorize(Roles = "Admin,Manager,Seller")]
        public async Task<ActionResult<ApiResponse<CarRentalDto>>> ReturnRental(int id, ReturnRentalDto model)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var isAdmin = await IsAdminOrManager();
                
                // Check if seller owns the car
                if (!isAdmin)
                {
                    var rental = await _rentalService.GetRentalByIdAsync(id);
                    if (rental == null)
                        return NotFound(new ApiResponse<CarRentalDto> { Success = false, Message = "Rental not found" });
                    
                    var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ProductId == rental.CarId);
                    if (car == null || car.SellerId != userId)
                        return Forbid();
                }
                
                var adminId = userId;
                var rentalResult = await _rentalService.ReturnRentalAsync(id, adminId, model);
                return Ok(new ApiResponse<CarRentalDto>
                {
                    Success = true,
                    Data = rentalResult,
                    Message = "Rental returned successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Rental not found"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning rental {Id}", id);
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Failed to return rental"
                });
            }
        }

        // POST: api/carrentals/{id}/extend
        [HttpPost("{id}/extend")]
        [Authorize(Roles = "Admin,Manager,Seller")]
        public async Task<ActionResult<ApiResponse<CarRentalDto>>> ExtendRental(int id, ExtendRentalDto model)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var isAdmin = await IsAdminOrManager();
                
                // Check if seller owns the car
                if (!isAdmin)
                {
                    var rental = await _rentalService.GetRentalByIdAsync(id);
                    if (rental == null)
                        return NotFound(new ApiResponse<CarRentalDto> { Success = false, Message = "Rental not found" });
                    
                    var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ProductId == rental.CarId);
                    if (car == null || car.SellerId != userId)
                        return Forbid();
                }
                
                var adminId = userId;
                var rentalResult = await _rentalService.ExtendRentalAsync(id, adminId, model);
                return Ok(new ApiResponse<CarRentalDto>
                {
                    Success = true,
                    Data = rentalResult,
                    Message = $"Rental extended by {model.AdditionalDays} days successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Rental not found"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending rental {Id}", id);
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Failed to extend rental"
                });
            }
        }

        // POST: api/carrentals/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<CarRentalDto>>> CancelRental(int id, [FromBody] string reason)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var rental = await _rentalService.CancelRentalAsync(id, userId, reason);
                return Ok(new ApiResponse<CarRentalDto>
                {
                    Success = true,
                    Data = rental,
                    Message = "Rental cancelled successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Rental not found"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling rental {Id}", id);
                return BadRequest(new ApiResponse<CarRentalDto>
                {
                    Success = false,
                    Message = "Failed to cancel rental"
                });
            }
        }

        // GET: api/carrentals/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<RentalStatisticsDto>>> GetStatistics()
        {
            try
            {
                var stats = await _rentalService.GetRentalStatisticsAsync();
                return Ok(new ApiResponse<RentalStatisticsDto> { Success = true, Data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rental statistics");
                return BadRequest(new ApiResponse<RentalStatisticsDto>
                {
                    Success = false,
                    Message = "Failed to retrieve statistics"
                });
            }
        }

        // GET: api/carrentals/calendar
        [HttpGet("calendar")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<List<CalendarEventDto>>>> GetCalendarEvents(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            try
            {
                var events = await _rentalService.GetCalendarEventsAsync(start, end);
                return Ok(new ApiResponse<List<CalendarEventDto>> { Success = true, Data = events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting calendar events");
                return BadRequest(new ApiResponse<List<CalendarEventDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve calendar events"
                });
            }
        }

        // GET: api/carrentals/my-calendar
        [HttpGet("my-calendar")]
        public async Task<ActionResult<ApiResponse<List<CalendarEventDto>>>> GetMyCalendarEvents(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var events = await _rentalService.GetUserCalendarEventsAsync(userId, start, end);
                return Ok(new ApiResponse<List<CalendarEventDto>> { Success = true, Data = events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user calendar events");
                return BadRequest(new ApiResponse<List<CalendarEventDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve calendar events"
                });
            }
        }

        // GET: api/carrentals/seller-calendar
        [HttpGet("seller-calendar")]
        [Authorize(Roles = "Seller,Admin,Manager")]
        public async Task<ActionResult<ApiResponse<List<CalendarEventDto>>>> GetSellerCalendarEvents(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var isAdmin = await IsAdminOrManager();
                
                // Sellers can only view their own calendar, admins can view any seller's calendar
                // For now, we'll use the current user's ID (sellers see their own, admins can be extended later)
                var events = await _rentalService.GetSellerCalendarEventsAsync(userId, start, end);
                return Ok(new ApiResponse<List<CalendarEventDto>> { Success = true, Data = events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seller calendar events");
                return BadRequest(new ApiResponse<List<CalendarEventDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve calendar events"
                });
            }
        }

        // GET: api/carrentals/check-availability
        [HttpGet("check-availability")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckAvailability(
            [FromQuery] int carId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? excludeRentalId = null)
        {
            try
            {
                var isAvailable = await _rentalService.IsCarAvailableForRentalAsync(carId, startDate, endDate, excludeRentalId);
                return Ok(new ApiResponse<bool> { Success = true, Data = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking car availability");
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to check availability"
                });
            }
        }
    }
}