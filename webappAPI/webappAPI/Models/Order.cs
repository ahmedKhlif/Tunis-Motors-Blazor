using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webappAPI.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "Confirmed")]
        Confirmed,
        [Display(Name = "Processing")]
        Processing,
        [Display(Name = "Shipped")]
        Shipped,
        [Display(Name = "Delivered")]
        Delivered,
        [Display(Name = "Cancelled")]
        Cancelled,
        [Display(Name = "Refunded")]
        Refunded
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string? Notes { get; set; }

        public DateTime? StatusUpdatedAt { get; set; }

        public string? StatusUpdatedBy { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        // Navigation property
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        // Helper properties
        public string StatusDisplay => Status.ToString();
        public string StatusColor => Status switch
        {
            OrderStatus.Pending => "warning",
            OrderStatus.Confirmed => "info",
            OrderStatus.Processing => "primary",
            OrderStatus.Shipped => "success",
            OrderStatus.Delivered => "success",
            OrderStatus.Cancelled => "danger",
            OrderStatus.Refunded => "secondary",
            _ => "secondary"
        };

        public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
        public bool CanBeUpdated => Status != OrderStatus.Delivered && Status != OrderStatus.Cancelled && Status != OrderStatus.Refunded;
    }
}
