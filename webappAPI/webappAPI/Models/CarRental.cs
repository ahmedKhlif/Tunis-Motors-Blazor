using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webappAPI.Models
{
    public enum RentalStatus
    {
        REQUESTED,
        APPROVED,
        ACTIVE,
        OVERDUE,
        RETURNED,
        CANCELLED
    }

    public class CarRental
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public virtual CarListing Car { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        [Required]
        public RentalStatus Status { get; set; } = RentalStatus.REQUESTED;

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }

        public int ExtendedCount { get; set; } = 0;

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Additional tracking fields
        public decimal? DailyRate { get; set; }
        public int? InitialMileage { get; set; }
        public int? ReturnMileage { get; set; }
        public string? DamageNotes { get; set; }
        public decimal? LateFees { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Helper Methods
        public string GetFormattedRequestedDate()
        {
            return RequestedAt.ToString("MMM dd, yyyy HH:mm");
        }

        public string GetFormattedApprovedDate()
        {
            return ApprovedAt?.ToString("MMM dd, yyyy HH:mm") ?? "Not approved";
        }

        public string GetFormattedPickupDate()
        {
            return PickupDate?.ToString("MMM dd, yyyy HH:mm") ?? "Not picked up";
        }

        public string GetFormattedReturnDueDate()
        {
            return ReturnDueDate?.ToString("MMM dd, yyyy") ?? "Not set";
        }

        public string GetFormattedReturnedDate()
        {
            return ReturnedAt?.ToString("MMM dd, yyyy HH:mm") ?? "Not returned";
        }

        public int GetRemainingDays()
        {
            if (ReturnDueDate == null || Status == RentalStatus.RETURNED || Status == RentalStatus.CANCELLED)
                return 0;

            var today = DateTime.UtcNow.Date;
            var dueDate = ReturnDueDate.Value.Date;

            return (dueDate - today).Days;
        }

        public int GetOverdueDays()
        {
            if (ReturnDueDate == null || Status != RentalStatus.ACTIVE)
                return 0;

            var today = DateTime.UtcNow.Date;
            var dueDate = ReturnDueDate.Value.Date;

            return today > dueDate ? (today - dueDate).Days : 0;
        }

        public bool IsOverdue()
        {
            return GetOverdueDays() > 0 && Status == RentalStatus.ACTIVE;
        }

        public decimal CalculateTotalCost()
        {
            if (PickupDate == null || ReturnDueDate == null)
                return 0;

            var days = (ReturnDueDate.Value - PickupDate.Value).Days;
            if (days <= 0) days = 1; // Minimum 1 day

            var baseCost = (DailyRate ?? 0) * days;
            var lateFees = LateFees ?? 0;

            return baseCost + lateFees;
        }

        public void ExtendRental(int additionalDays)
        {
            if (ReturnDueDate != null)
            {
                ReturnDueDate = ReturnDueDate.Value.AddDays(additionalDays);
                ExtendedCount++;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}