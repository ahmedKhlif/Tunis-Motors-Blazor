using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Services;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;
        private readonly AppDbContext _context;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger, AppDbContext context)
        {
            _messageService = messageService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("inbox")]
        public async Task<ActionResult<ApiResponse<List<MessageDto>>>> GetInbox()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = await _messageService.GetInboxAsync(userId);

            return Ok(new ApiResponse<List<MessageDto>> { Success = true, Data = messages });
        }

        [HttpGet("sent")]
        public async Task<ActionResult<ApiResponse<List<MessageDto>>>> GetSentMessages()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = await _messageService.GetSentMessagesAsync(userId);

            return Ok(new ApiResponse<List<MessageDto>> { Success = true, Data = messages });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MessageDto>>> GetMessage(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
                return NotFound(new ApiResponse { Success = false, Message = "Message not found" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (message.SenderId != userId && message.ReceiverId != userId)
                return Forbid();

            return Ok(new ApiResponse<MessageDto> { Success = true, Data = message });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage([FromBody] CreateMessageDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var message = await _messageService.SendMessageAsync(userId, model);

            if (message == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to send message" });

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id },
                new ApiResponse<MessageDto> { Success = true, Data = message, Message = "Message sent successfully" });
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
                return NotFound(new ApiResponse { Success = false, Message = "Message not found" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (message.ReceiverId != userId)
                return Forbid();

            var result = await _messageService.MarkAsReadAsync(id);
            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to mark as read" });

            return Ok(new ApiResponse { Success = true, Message = "Marked as read" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteMessage(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
                return NotFound(new ApiResponse { Success = false, Message = "Message not found" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (message.SenderId != userId && message.ReceiverId != userId)
                return Forbid();

            var result = await _messageService.DeleteMessageAsync(id);
            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to delete message" });

            return Ok(new ApiResponse { Success = true, Message = "Message deleted successfully" });
        }

        [HttpGet("recipients")]
        public async Task<ActionResult<ApiResponse<List<UserWithRoleDto>>>> GetMessageRecipients()
        {
            try
            {
                var usersWithRoles = await (from user in _context.Users
                                           join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                           join role in _context.Roles on userRole.RoleId equals role.Id
                                           where role.Name != "Buyer" // Exclude buyers from recipient list
                                           select new UserWithRoleDto
                                           {
                                               Id = user.Id,
                                               UserName = user.UserName,
                                               Email = user.Email,
                                               Role = role.Name
                                           }).ToListAsync();

                return Ok(new ApiResponse<List<UserWithRoleDto>>
                {
                    Success = true,
                    Data = usersWithRoles,
                    Message = $"{usersWithRoles.Count} recipients available"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting recipients: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving recipients" });
            }
        }

        [HttpGet("unread/count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
                }

                var unreadCount = await _context.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead);

                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = unreadCount,
                    Message = $"{unreadCount} unread messages"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting unread count: {ex.Message}");
                return BadRequest(new ApiResponse<int> { Success = false, Message = "Error retrieving unread count", Data = 0 });
            }
        }
    }
}
