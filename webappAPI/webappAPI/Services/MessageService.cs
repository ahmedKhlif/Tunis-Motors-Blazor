using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MessageService> _logger;

        public MessageService(AppDbContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MessageDto> GetMessageByIdAsync(int id)
        {
            try
            {
                var message = await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Include(m => m.Listing)
                    .FirstOrDefaultAsync(m => m.Id == id);

                return message != null ? MapToDto(message) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting message: {ex.Message}");
                return null;
            }
        }

        public async Task<List<MessageDto>> GetInboxAsync(string userId)
        {
            try
            {
                var messages = await _context.Messages
                    .Where(m => m.ReceiverId == userId)
                    .Include(m => m.Sender)
                    .Include(m => m.Listing)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();

                return messages.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting inbox: {ex.Message}");
                return new List<MessageDto>();
            }
        }

        public async Task<List<MessageDto>> GetSentMessagesAsync(string userId)
        {
            try
            {
                var messages = await _context.Messages
                    .Where(m => m.SenderId == userId)
                    .Include(m => m.Receiver)
                    .Include(m => m.Listing)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();

                return messages.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting sent messages: {ex.Message}");
                return new List<MessageDto>();
            }
        }

        public async Task<MessageDto> SendMessageAsync(string senderId, CreateMessageDto model)
        {
            try
            {
                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = model.ReceiverId,
                    Subject = model.Subject,
                    Content = model.Content,
                    ListingId = model.ListingId,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return await GetMessageByIdAsync(message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> MarkAsReadAsync(int messageId)
        {
            try
            {
                var message = await _context.Messages.FindAsync(messageId);
                if (message == null)
                    return false;

                message.IsRead = true;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking message as read: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            try
            {
                var message = await _context.Messages.FindAsync(id);
                if (message == null)
                    return false;

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting message: {ex.Message}");
                return false;
            }
        }

        private MessageDto MapToDto(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                SenderName = message.Sender?.UserName ?? "Unknown",
                ReceiverName = message.Receiver?.UserName ?? "Unknown",
                Subject = message.Subject,
                Content = message.Content,
                IsRead = message.IsRead,
                SentAt = message.SentAt,
                ListingId = message.ListingId
            };
        }
    }
}
