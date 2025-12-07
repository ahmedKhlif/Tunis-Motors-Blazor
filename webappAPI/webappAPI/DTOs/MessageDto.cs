using System.ComponentModel.DataAnnotations;

namespace webappAPI.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int? ListingId { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class CreateMessageDto
    {
        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        public int? ListingId { get; set; }
    }

    public class MarkMessageAsReadDto
    {
        [Required]
        public int MessageId { get; set; }
    }
}
