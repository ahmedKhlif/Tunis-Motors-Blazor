using System.ComponentModel.DataAnnotations;

namespace webappAPI.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(8000000)]
        public string? Image { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(8000000)]
        public string? Image { get; set; }
    }
}
