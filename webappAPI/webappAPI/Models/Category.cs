using System.ComponentModel.DataAnnotations;

namespace webappAPI.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; }

        // Optional description (added to align with Blazor front-end model)
        [StringLength(1000)]
        public string? Description { get; set; }

        // Store image data or path. Previous limit of 500 was too small for base64 data URIs.
        // NOTE: Large base64 strings can bloat the database; consider switching to file upload + path storage later.
        [StringLength(8000000)] // Allows ~7MB base64 string for a 5MB image
        public string? Image { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<CarListing> CarListings { get; set; } = new List<CarListing>();
    }
}
