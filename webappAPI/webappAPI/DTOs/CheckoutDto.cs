using System.ComponentModel.DataAnnotations;

namespace webappAPI.DTOs
{
    public class CheckoutDto
    {
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(100)]
        public string PaymentMethod { get; set; } // "CreditCard", "Stripe", "BankTransfer", etc.

        public string? StripeToken { get; set; } // For Stripe payments

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
    }

    public class ApproveListingDto
    {
        [StringLength(500)]
        public string? AdminNote { get; set; }
    }

    public class RejectListingDto
    {
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string RejectionReason { get; set; }
    }
}
