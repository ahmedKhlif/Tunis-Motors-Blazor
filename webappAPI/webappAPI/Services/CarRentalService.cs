using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class CarRentalService : ICarRentalService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CarRentalService> _logger;
        private readonly IEmailService _emailService;

        public CarRentalService(AppDbContext context, ILogger<CarRentalService> _logger, IEmailService emailService)
        {
            _context = context;
            this._logger = _logger;
            _emailService = emailService;
        }

        public async Task<CarRentalDto> GetRentalByIdAsync(int id)
        {
            var rental = await _context.CarRentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
                return null;

            return MapToDto(rental);
        }

        public async Task<PaginatedDto<CarRentalDto>> GetAllRentalsAsync(CarRentalFilterDto filter)
        {
            var query = _context.CarRentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);

            if (!string.IsNullOrEmpty(filter.UserId))
                query = query.Where(r => r.UserId == filter.UserId);

            if (filter.CarId.HasValue)
                query = query.Where(r => r.CarId == filter.CarId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(r => r.RequestedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.RequestedAt <= filter.EndDate.Value);

            if (filter.IsOverdue.HasValue && filter.IsOverdue.Value)
                query = query.Where(r => r.Status == RentalStatus.ACTIVE &&
                                        r.ReturnDueDate < DateTime.UtcNow);

            // Order by most recent first
            query = query.OrderByDescending(r => r.RequestedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedDto<CarRentalDto>
            {
                Data = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<CarRentalDto> CreateRentalRequestAsync(string userId, CreateCarRentalDto model)
        {
            // Validate car availability
            if (!await IsCarAvailableForRentalAsync(model.CarId, model.PickupDate, model.ReturnDueDate))
                throw new InvalidOperationException("Car is not available for the selected dates");

            var rental = new CarRental
            {
                CarId = model.CarId,
                UserId = userId,
                PickupDate = model.PickupDate,
                ReturnDueDate = model.ReturnDueDate,
                Notes = model.Notes,
                Status = RentalStatus.REQUESTED
            };

            _context.CarRentals.Add(rental);
            await _context.SaveChangesAsync();

            // Send confirmation email to user
            var rentalDetails = await GetRentalByIdAsync(rental.Id);
            try
            {
                await _emailService.SendRentalRequestNotificationAsync(
                    rentalDetails.UserEmail,
                    rentalDetails.UserName,
                    rentalDetails.CarName,
                    rentalDetails.PickupDate.Value,
                    rentalDetails.ReturnDueDate.Value
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rental request email for rental {RentalId}", rental.Id);
            }

            return rentalDetails;
        }

        public async Task<CarRentalDto> UpdateRentalAsync(int id, UpdateCarRentalDto model)
        {
            var rental = await _context.CarRentals.FindAsync(id);
            if (rental == null)
                throw new KeyNotFoundException("Rental not found");

            if (model.PickupDate.HasValue)
                rental.PickupDate = model.PickupDate.Value;

            if (model.ReturnDueDate.HasValue)
                rental.ReturnDueDate = model.ReturnDueDate.Value;

            if (model.InitialMileage.HasValue)
                rental.InitialMileage = model.InitialMileage.Value;

            if (!string.IsNullOrEmpty(model.Notes))
                rental.Notes = model.Notes;

            if (model.DailyRate.HasValue)
                rental.DailyRate = model.DailyRate.Value;

            rental.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetRentalByIdAsync(id);
        }

        public async Task<CarRentalDto> ApproveRentalAsync(int id, string adminId, ApproveRentalDto model)
        {
            var rental = await _context.CarRentals
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
                throw new KeyNotFoundException("Rental not found");

            if (rental.Status != RentalStatus.REQUESTED)
                throw new InvalidOperationException("Only requested rentals can be approved");

            rental.Status = RentalStatus.APPROVED;
            rental.ApprovedAt = DateTime.UtcNow;
            rental.ApprovedBy = adminId;
            rental.DailyRate = model.DailyRate;
            rental.Notes = model.Notes ?? rental.Notes;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send approval email to user
            try
            {
                await _emailService.SendRentalApprovedNotificationAsync(
                    rental.User.Email,
                    rental.User.UserName ?? rental.User.Email,
                    rental.Car.Name,
                    rental.PickupDate.Value,
                    rental.ReturnDueDate.Value
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rental approved email for rental {RentalId}", id);
            }
            _logger.LogInformation($"Rental {id} approved by {adminId}");

            return await GetRentalByIdAsync(id);
        }

        public async Task<CarRentalDto> RejectRentalAsync(int id, string adminId, string reason)
        {
            var rental = await _context.CarRentals.FindAsync(id);
            if (rental == null)
                throw new KeyNotFoundException("Rental not found");

            if (rental.Status != RentalStatus.REQUESTED)
                throw new InvalidOperationException("Only requested rentals can be rejected");

            rental.Status = RentalStatus.CANCELLED;
            rental.Notes = $"{rental.Notes}\n\nRejected by {adminId}: {reason}".Trim();
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send rejection email to user
            var rentalDetails = await GetRentalByIdAsync(id);
            try
            {
                await _emailService.SendRentalRejectedNotificationAsync(
                    rentalDetails.UserEmail,
                    rentalDetails.UserName,
                    rentalDetails.CarName,
                    reason
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rental rejected email for rental {RentalId}", id);
            }
            _logger.LogInformation($"Rental {id} rejected by {adminId}");

            return rentalDetails;
        }

        public async Task<CarRentalDto> ActivateRentalAsync(int id, string adminId)
        {
            var rental = await _context.CarRentals.FindAsync(id);
            if (rental == null)
                throw new KeyNotFoundException("Rental not found");

            if (rental.Status != RentalStatus.APPROVED)
                throw new InvalidOperationException("Only approved rentals can be activated");

            rental.Status = RentalStatus.ACTIVE;
            rental.PickupDate = DateTime.UtcNow;
            rental.UpdatedAt = DateTime.UtcNow;

            // Decrement rental stock
            var car = await _context.CarListings.FindAsync(rental.CarId);
            if (car != null && car.RentalStock > 0)
            {
                car.RentalStock--;
                if (car.RentalStock <= 0)
                {
                    car.IsAvailableForRental = false;
                }
                car.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rental {id} activated by {adminId}. Stock remaining: {car?.RentalStock}");

            return await GetRentalByIdAsync(id);
        }

        public async Task<CarRentalDto> ReturnRentalAsync(int id, string adminId, ReturnRentalDto model)
        {
            var rental = await _context.CarRentals.FindAsync(id);
            if (rental == null)
                throw new KeyNotFoundException("Rental not found");

            if (rental.Status != RentalStatus.ACTIVE)
                throw new InvalidOperationException("Only active rentals can be returned");

            rental.Status = RentalStatus.RETURNED;
            rental.ReturnedAt = DateTime.UtcNow;
            rental.ReturnMileage = model.ReturnMileage;
            rental.DamageNotes = model.DamageNotes;
            rental.LateFees = model.LateFees;
            rental.UpdatedAt = DateTime.UtcNow;

            // Increment rental stock
            var car = await _context.CarListings.FindAsync(rental.CarId);
            if (car != null)
            {
                car.RentalStock++;
                if (!car.IsAvailableForRental && car.RentalStock > 0)
                {
                    car.IsAvailableForRental = true;
                }
                car.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Send completion email to user
            var rentalDetails = await GetRentalByIdAsync(id);
            try
            {
                await _emailService.SendRentalReturnedNotificationAsync(
                    rentalDetails.UserEmail,
                    rentalDetails.UserName,
                    rentalDetails.CarName,
                    rentalDetails.TotalCost
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rental returned email for rental {RentalId}", id);
            }

            _logger.LogInformation($"Rental {id} returned by {adminId}. Stock restored: {car?.RentalStock}");

            return rentalDetails;
        }

        public async Task<CarRentalDto> ExtendRentalAsync(int id, string adminId, ExtendRentalDto model)
        {
            var rental = await _context.CarRentals.FindAsync(id);
            if (rental == null)
                throw new KeyNotFoundException("Rental not found");

            if (rental.Status != RentalStatus.ACTIVE)
                throw new InvalidOperationException("Only active rentals can be extended");

            rental.ExtendRental(model.AdditionalDays);
            rental.Notes = $"{rental.Notes}\n\nExtended by {model.AdditionalDays} days by {adminId}. Reason: {model.Reason}".Trim();

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rental {id} extended by {model.AdditionalDays} days by {adminId}");

            return await GetRentalByIdAsync(id);
        }

        public async Task<CarRentalDto> CancelRentalAsync(int id, string userId, string reason)
        {
            var rental = await _context.CarRentals.FindAsync(id);
            if (rental == null)
                throw new KeyNotFoundException("Rental not found");

            if (rental.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own rentals");

            if (rental.Status != RentalStatus.REQUESTED && rental.Status != RentalStatus.APPROVED && rental.Status != RentalStatus.ACTIVE)
                throw new InvalidOperationException("Only requested, approved, or active rentals can be cancelled");

            // If rental was active, restore stock
            if (rental.Status == RentalStatus.ACTIVE)
            {
                var car = await _context.CarListings.FindAsync(rental.CarId);
                if (car != null)
                {
                    car.RentalStock++;
                    if (!car.IsAvailableForRental && car.RentalStock > 0)
                    {
                        car.IsAvailableForRental = true;
                    }
                    car.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Restored rental stock for car {car.ProductId}. New stock: {car.RentalStock}");
                }
            }

            rental.Status = RentalStatus.CANCELLED;
            rental.Notes = $"{rental.Notes}\n\nCancelled by user: {reason}".Trim();
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rental {id} cancelled by user {userId}");

            return await GetRentalByIdAsync(id);
        }

        public async Task<RentalStatisticsDto> GetRentalStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);

            var rentals = await _context.CarRentals.ToListAsync();

            return new RentalStatisticsDto
            {
                TotalRentals = rentals.Count,
                PendingRequests = rentals.Count(r => r.Status == RentalStatus.REQUESTED),
                ActiveRentals = rentals.Count(r => r.Status == RentalStatus.ACTIVE),
                OverdueRentals = rentals.Count(r => r.Status == RentalStatus.OVERDUE ||
                                                   (r.Status == RentalStatus.ACTIVE && r.ReturnDueDate < now)),
                ReturnedToday = rentals.Count(r => r.Status == RentalStatus.RETURNED &&
                                                 r.ReturnedAt?.Date == now.Date),
                ReturnedThisWeek = rentals.Count(r => r.Status == RentalStatus.RETURNED &&
                                                    r.ReturnedAt >= startOfWeek),
                ReturnedThisMonth = rentals.Count(r => r.Status == RentalStatus.RETURNED &&
                                                     r.ReturnedAt >= startOfMonth),
                TotalRevenue = rentals.Where(r => r.Status == RentalStatus.RETURNED)
                                    .Sum(r => r.CalculateTotalCost()),
                MonthlyRevenue = rentals.Where(r => r.Status == RentalStatus.RETURNED &&
                                                  r.ReturnedAt >= startOfMonth)
                                       .Sum(r => r.CalculateTotalCost()),
                AverageRentalDuration = rentals.Where(r => r.Status == RentalStatus.RETURNED &&
                                                        r.PickupDate.HasValue && r.ReturnedAt.HasValue)
                                             .DefaultIfEmpty()
                                             .Average(r => r != null ? (r.ReturnedAt.Value - r.PickupDate.Value).TotalDays : 0)
            };
        }

        public async Task<List<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end)
        {
            var rentals = await _context.CarRentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .Where(r => r.Status != RentalStatus.CANCELLED &&
                           ((r.PickupDate >= start && r.PickupDate <= end) ||
                            (r.ReturnDueDate >= start && r.ReturnDueDate <= end)))
                .ToListAsync();

            var events = new List<CalendarEventDto>();

            foreach (var rental in rentals)
            {
                if (rental.PickupDate.HasValue && rental.ReturnDueDate.HasValue)
                {
                    events.Add(new CalendarEventDto
                    {
                        Id = rental.Id,
                        Title = $"{rental.Car?.Name ?? "Car"} - {rental.User?.UserName ?? "User"}",
                        Start = rental.PickupDate.Value,
                        End = rental.ReturnDueDate.Value,
                        Color = GetStatusColor(rental.Status),
                        Status = rental.Status.ToString(),
                        CarName = rental.Car?.Name ?? "Unknown Car",
                        UserName = rental.User?.UserName ?? "Unknown User",
                        IsOverdue = rental.IsOverdue()
                    });
                }
            }

            return events;
        }

        public async Task<List<CalendarEventDto>> GetUserCalendarEventsAsync(string userId, DateTime start, DateTime end)
        {
            var rentals = await _context.CarRentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .Where(r => r.UserId == userId &&
                           r.Status != RentalStatus.CANCELLED &&
                           ((r.PickupDate >= start && r.PickupDate <= end) ||
                            (r.ReturnDueDate >= start && r.ReturnDueDate <= end)))
                .ToListAsync();

            var events = new List<CalendarEventDto>();

            foreach (var rental in rentals)
            {
                if (rental.PickupDate.HasValue && rental.ReturnDueDate.HasValue)
                {
                    events.Add(new CalendarEventDto
                    {
                        Id = rental.Id,
                        Title = $"{rental.Car?.Name ?? "Car"} - {GetStatusDisplayName(rental.Status)}",
                        Start = rental.PickupDate.Value,
                        End = rental.ReturnDueDate.Value,
                        Color = GetStatusColor(rental.Status),
                        Status = rental.Status.ToString(),
                        CarName = rental.Car?.Name ?? "Unknown Car",
                        UserName = rental.User?.UserName ?? "Unknown User",
                        IsOverdue = rental.IsOverdue()
                    });
                }
            }

            return events;
        }

        public async Task<List<CalendarEventDto>> GetSellerCalendarEventsAsync(string sellerId, DateTime start, DateTime end)
        {
            var rentals = await _context.CarRentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .Where(r => r.Car.SellerId == sellerId &&
                           r.Status != RentalStatus.CANCELLED &&
                           ((r.PickupDate >= start && r.PickupDate <= end) ||
                            (r.ReturnDueDate >= start && r.ReturnDueDate <= end)))
                .ToListAsync();

            var events = new List<CalendarEventDto>();

            foreach (var rental in rentals)
            {
                if (rental.PickupDate.HasValue && rental.ReturnDueDate.HasValue)
                {
                    events.Add(new CalendarEventDto
                    {
                        Id = rental.Id,
                        Title = $"{rental.Car?.Name ?? "Car"} - {rental.User?.UserName ?? "Customer"} - {GetStatusDisplayName(rental.Status)}",
                        Start = rental.PickupDate.Value,
                        End = rental.ReturnDueDate.Value,
                        Color = GetStatusColor(rental.Status),
                        Status = rental.Status.ToString(),
                        CarName = rental.Car?.Name ?? "Unknown Car",
                        UserName = rental.User?.UserName ?? "Unknown User",
                        IsOverdue = rental.IsOverdue()
                    });
                }
            }

            return events;
        }

        public async Task<PaginatedDto<CarRentalDto>> GetUserRentalsAsync(string userId, CarRentalFilterDto filter)
        {
            filter.UserId = userId;
            return await GetAllRentalsAsync(filter);
        }

        public async Task<List<CarRentalDto>> GetSellerRentalsAsync(string sellerId)
        {
            var rentals = await _context.CarRentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .Where(r => r.Car.SellerId == sellerId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return rentals.Select(MapToDto).ToList();
        }

        public async Task<bool> IsCarAvailableForRentalAsync(int carId, DateTime startDate, DateTime endDate, int? excludeRentalId = null)
        {
            var conflictingRentals = await _context.CarRentals
                .Where(r => r.CarId == carId &&
                           r.Status != RentalStatus.CANCELLED &&
                           r.Status != RentalStatus.RETURNED &&
                           (excludeRentalId == null || r.Id != excludeRentalId))
                .Where(r => r.PickupDate.HasValue && r.ReturnDueDate.HasValue &&
                           ((startDate >= r.PickupDate && startDate <= r.ReturnDueDate) ||
                            (endDate >= r.PickupDate && endDate <= r.ReturnDueDate) ||
                            (startDate <= r.PickupDate && endDate >= r.ReturnDueDate)))
                .CountAsync();

            return conflictingRentals == 0;
        }

        public async Task<decimal> CalculateLateFeesAsync(int rentalId)
        {
            var rental = await _context.CarRentals.FindAsync(rentalId);
            if (rental == null) return 0;

            var overdueDays = rental.GetOverdueDays();
            if (overdueDays <= 0) return 0;

            // Assume $25 per day late fee
            return overdueDays * 25m;
        }

        public async Task ProcessOverdueRentalsAsync()
        {
            var overdueRentals = await _context.CarRentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .Where(r => r.Status == RentalStatus.ACTIVE &&
                           r.ReturnDueDate < DateTime.UtcNow)
                .ToListAsync();

            foreach (var rental in overdueRentals)
            {
                rental.Status = RentalStatus.OVERDUE;
                rental.LateFees = await CalculateLateFeesAsync(rental.Id);
                rental.UpdatedAt = DateTime.UtcNow;

                // Send overdue notification email
                try
                {
                    await _emailService.SendRentalOverdueNotificationAsync(
                        rental.User.Email,
                        rental.User.UserName ?? rental.User.Email,
                        rental.Car.Name,
                        rental.ReturnDueDate.Value
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send overdue email for rental {RentalId}", rental.Id);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Processed {overdueRentals.Count} overdue rentals");
        }

        private CarRentalDto MapToDto(CarRental rental)
        {
            return new CarRentalDto
            {
                Id = rental.Id,
                CarId = rental.CarId,
                CarName = rental.Car?.Name ?? "Unknown Car",
                CarImage = rental.Car?.Image ?? "/images/noimage.jpg",
                UserId = rental.UserId,
                UserName = rental.User?.UserName ?? "Unknown User",
                UserEmail = rental.User?.Email ?? "Unknown Email",
                Status = rental.Status,
                RequestedAt = rental.RequestedAt,
                ApprovedAt = rental.ApprovedAt,
                ApprovedBy = rental.ApprovedBy,
                PickupDate = rental.PickupDate,
                ReturnDueDate = rental.ReturnDueDate,
                ReturnedAt = rental.ReturnedAt,
                ExtendedCount = rental.ExtendedCount,
                Notes = rental.Notes,
                DailyRate = rental.DailyRate,
                InitialMileage = rental.InitialMileage,
                ReturnMileage = rental.ReturnMileage,
                DamageNotes = rental.DamageNotes,
                LateFees = rental.LateFees,
                RemainingDays = rental.GetRemainingDays(),
                OverdueDays = rental.GetOverdueDays(),
                IsOverdue = rental.IsOverdue(),
                TotalCost = rental.CalculateTotalCost()
            };
        }

        private string GetStatusColor(RentalStatus status)
        {
            return status switch
            {
                RentalStatus.REQUESTED => "#ffa726", // Orange
                RentalStatus.APPROVED => "#42a5f5",  // Blue
                RentalStatus.ACTIVE => "#66bb6a",    // Green
                RentalStatus.OVERDUE => "#ef5350",   // Red
                RentalStatus.RETURNED => "#26a69a",  // Teal
                RentalStatus.CANCELLED => "#78909c", // Grey
                _ => "#78909c"
            };
        }

        private string GetStatusDisplayName(RentalStatus status)
        {
            return status switch
            {
                RentalStatus.REQUESTED => "Requested",
                RentalStatus.APPROVED => "Approved",
                RentalStatus.ACTIVE => "Active",
                RentalStatus.OVERDUE => "Overdue",
                RentalStatus.RETURNED => "Returned",
                RentalStatus.CANCELLED => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}