using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webappAPI.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public int? ListingId { get; set; }

        // Navigation properties
        [ForeignKey("SenderId")]
        public virtual IdentityUser? Sender { get; set; }
    
        [ForeignKey("ReceiverId")]
        public virtual IdentityUser? Receiver { get; set; }
    
        [ForeignKey("ListingId")]
        public virtual CarListing? Listing { get; set; }
    }
}
