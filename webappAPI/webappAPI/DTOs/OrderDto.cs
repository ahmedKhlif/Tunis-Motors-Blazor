using System.ComponentModel.DataAnnotations;

namespace webappAPI.DTOs
{
    // Order DTOs
    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public string? StatusUpdatedBy { get; set; }
        public string UserId { get; set; }
        public List<OrderItemDto>? Items { get; set; }
    }

    public class CreateOrderDto
    {
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

        [Range(0, double.MaxValue)]
        public decimal? TotalAmount { get; set; }

        public List<CreateOrderItemDto>? Items { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // Order Item DTOs
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public CarListingDto? Product { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateOrderItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }


    // Purchase Request DTOs
    public class PurchaseRequestDto
    {
        public int PurchaseRequestId { get; set; }
        public string CustomerId { get; set; }
        public int ProductId { get; set; }
        public string Message { get; set; }
        public string? PhoneNumber { get; set; }
        public string Status { get; set; }
        public string? SellerResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public UserDto? Customer { get; set; }
        public CarListingDto? CarListing { get; set; }
    }

    public class CreatePurchaseRequestDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }

    public class UpdatePurchaseRequestDto
    {
        [StringLength(1000)]
        public string? SellerResponse { get; set; }

        public string? Status { get; set; }
    }

    public class ProcessPaymentDto
    {
        public string? PaymentIntentId { get; set; }
    }

}
