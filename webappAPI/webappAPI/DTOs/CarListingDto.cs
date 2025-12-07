using System.ComponentModel.DataAnnotations;

namespace webappAPI.DTOs
{

    // Car Listing DTOs
    public class CarListingDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Mileage { get; set; }
        public int Year { get; set; }
        public string Brand { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public string Color { get; set; }
        public string? VIN { get; set; }
        public decimal? EngineSize { get; set; }
        public int? Horsepower { get; set; }
        public int? Doors { get; set; }
        public int? Seats { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public string Condition { get; set; }
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public string? AdminApprovalNote { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public int Views { get; set; }
        public string Image { get; set; }
        public string? AdditionalImages { get; set; }
        public string? Location { get; set; }
        public int QuantityInStock { get; set; }
        public bool IsAvailableForRental { get; set; }
        public int RentalStock { get; set; }
        public decimal? DailyRentalRate { get; set; }
        public int? CategoryId { get; set; }
        public string? SellerId { get; set; }
        public UserDto? Seller { get; set; }
        public CategoryDto? Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCarListingDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        [Required]
        [Range(1900, 2030)]
        public int Year { get; set; }

        [Required]
        [StringLength(50)]
        public string Brand { get; set; }

        [Required]
        [StringLength(50)]
        public string FuelType { get; set; }

        [Required]
        [StringLength(50)]
        public string Transmission { get; set; }

        [Required]
        [StringLength(50)]
        public string Color { get; set; }

        [StringLength(17)]
        public string? VIN { get; set; }

        [Range(0, 10)]
        public decimal? EngineSize { get; set; }

        [Range(0, 2000)]
        public int? Horsepower { get; set; }

        [Range(2, 5)]
        public int? Doors { get; set; }

        [Range(2, 9)]
        public int? Seats { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Features { get; set; }

        [Required]
        public string Condition { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required]
        public string Image { get; set; }

        public string? AdditionalImages { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityInStock { get; set; }

        public bool IsAvailableForRental { get; set; } = false;

        [Range(0, int.MaxValue)]
        public int RentalStock { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal? DailyRentalRate { get; set; }

        public int? CategoryId { get; set; }
    }

    public class UpdateCarListingDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? Mileage { get; set; }

        [Range(1900, 2030)]
        public int? Year { get; set; }

        [StringLength(50)]
        public string? Brand { get; set; }

        [StringLength(50)]
        public string? FuelType { get; set; }

        [StringLength(50)]
        public string? Transmission { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(17)]
        public string? VIN { get; set; }

        [Range(0, 10)]
        public decimal? EngineSize { get; set; }

        [Range(0, 2000)]
        public int? Horsepower { get; set; }

        [Range(2, 5)]
        public int? Doors { get; set; }

        [Range(2, 9)]
        public int? Seats { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Features { get; set; }

        public string? Condition { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        public string? Image { get; set; }

        public string? AdditionalImages { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [Range(0, int.MaxValue)]
        public int? QuantityInStock { get; set; }

        public bool? IsAvailableForRental { get; set; }

        [Range(0, int.MaxValue)]
        public int? RentalStock { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? DailyRentalRate { get; set; }

        public int? CategoryId { get; set; }
    }

    public class CarListingFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public int? MaxMileage { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public string? Color { get; set; }
        public int? MinRating { get; set; }
        public int SortBy { get; set; } = 0; // 0: Newest, 1: Price Low-High, 2: Price High-Low, 3: Most Popular
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public bool IncludeUnapproved { get; set; } = false;
    }


}


