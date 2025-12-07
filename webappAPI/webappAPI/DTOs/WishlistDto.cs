using System.ComponentModel.DataAnnotations;

namespace webappAPI.DTOs
{
    public class WishlistDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public CarListingDto CarListing { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class AddToWishlistDto
    {
        [Required]
        public int ProductId { get; set; }
    }
}
