using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webappAPI.Models
{
    public class CarListing
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Car Model")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Price (TND)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Mileage (km)")]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        [Required]
        [Display(Name = "Year")]
        [Range(1900, 2030)]
        public int Year { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Brand")]
        public string Brand { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Transmission")]
        public string Transmission { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Color")]
        public string Color { get; set; }

        [StringLength(17)]
        [Display(Name = "VIN Number")]
        public string? VIN { get; set; }

        [Display(Name = "Engine Size (L)")]
        [Range(0, 10)]
        public decimal? EngineSize { get; set; }

        [Display(Name = "Horsepower")]
        [Range(0, 2000)]
        public int? Horsepower { get; set; }

        [Display(Name = "Number of Doors")]
        [Range(2, 5)]
        public int? Doors { get; set; }

        [Display(Name = "Number of Seats")]
        [Range(2, 9)]
        public int? Seats { get; set; }

        [Display(Name = "Description")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Features")]
        [StringLength(500)]
        public string? Features { get; set; }

        [Required]
        [Display(Name = "Condition")]
        public string Condition { get; set; } = "Used"; // New, Used, Certified Pre-Owned

        [Required]
        [Display(Name = "Rating (Stars)")]
        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        public string? SellerId { get; set; }
        [ForeignKey("SellerId")]
        public virtual IdentityUser? Seller { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // Approval workflow
        public bool IsApproved { get; set; } = false;
        public string? AdminApprovalNote { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        [Required]
        [Display(Name = "Main Image")]
        public string Image { get; set; }

        [Display(Name = "Additional Images")]
        public string? AdditionalImages { get; set; } // JSON array of image paths

        [Display(Name = "Location")]
        [StringLength(100)]
        public string? Location { get; set; }

        [Required]
        [Display(Name = "Quantity in Stock")]
        [Range(0, int.MaxValue)]
        public int QuantityInStock { get; set; } = 0;

        [Display(Name = "Available for Rental")]
        public bool IsAvailableForRental { get; set; } = false;

        [Display(Name = "Rental Stock")]
        [Range(0, int.MaxValue)]
        public int RentalStock { get; set; } = 0;

        [Display(Name = "Daily Rental Rate (TND)")]
        [Range(0, double.MaxValue)]
        public decimal? DailyRentalRate { get; set; }

        [Display(Name = "View Count")]
        public int Views { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
