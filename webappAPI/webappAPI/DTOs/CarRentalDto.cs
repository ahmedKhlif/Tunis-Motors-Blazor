using System.ComponentModel.DataAnnotations;
using webappAPI.Models;

namespace webappAPI.DTOs
{
    // Car Rental DTOs
    public class CarRentalDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CarName { get; set; }
        public string CarImage { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public RentalStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public int ExtendedCount { get; set; }
        public string? Notes { get; set; }
        public decimal? DailyRate { get; set; }
        public int? InitialMileage { get; set; }
        public int? ReturnMileage { get; set; }
        public string? DamageNotes { get; set; }
        public decimal? LateFees { get; set; }

        // Computed properties
        public string FormattedRequestedDate => RequestedAt.ToString("MMM dd, yyyy HH:mm");
        public string FormattedApprovedDate => ApprovedAt?.ToString("MMM dd, yyyy HH:mm") ?? "Not approved";
        public string FormattedPickupDate => PickupDate?.ToString("MMM dd, yyyy HH:mm") ?? "Not picked up";
        public string FormattedReturnDueDate => ReturnDueDate?.ToString("MMM dd, yyyy") ?? "Not set";
        public string FormattedReturnedDate => ReturnedAt?.ToString("MMM dd, yyyy HH:mm") ?? "Not returned";
        public int RemainingDays { get; set; }
        public int OverdueDays { get; set; }
        public bool IsOverdue { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class CreateCarRentalDto
    {
        [Required]
        public int CarId { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime ReturnDueDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateCarRentalDto
    {
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDueDate { get; set; }
        public int? InitialMileage { get; set; }
        public string? Notes { get; set; }
        public decimal? DailyRate { get; set; }
    }

    public class ApproveRentalDto
    {
        [Required]
        public decimal DailyRate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class ReturnRentalDto
    {
        public int? ReturnMileage { get; set; }

        [StringLength(1000)]
        public string? DamageNotes { get; set; }

        public decimal? LateFees { get; set; } = 0;
    }

    public class ExtendRentalDto
    {
        [Required]
        [Range(1, 30)]
        public int AdditionalDays { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }

    public class RentalStatisticsDto
    {
        public int TotalRentals { get; set; }
        public int PendingRequests { get; set; }
        public int ActiveRentals { get; set; }
        public int OverdueRentals { get; set; }
        public int ReturnedToday { get; set; }
        public int ReturnedThisWeek { get; set; }
        public int ReturnedThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public double AverageRentalDuration { get; set; }
    }

    public class CarRentalFilterDto
    {
        public RentalStatus? Status { get; set; }
        public string? UserId { get; set; }
        public int? CarId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsOverdue { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Color { get; set; }
        public string Status { get; set; }
        public string CarName { get; set; }
        public string UserName { get; set; }
        public bool IsOverdue { get; set; }
    }
}