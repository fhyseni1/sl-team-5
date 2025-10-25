using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserHealthService.Application.DTOs.Chat;
using UserHealthService.Application.Interfaces;
using UserHealthService.Infrastructure.Data; 

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly UserHealthDbContext _context; 

        public ChatController(
            IChatService chatService, 
            ILogger<ChatController> logger,
            UserHealthDbContext context) 
        {
            _chatService = chatService;
            _logger = logger;
            _context = context; 
        }

[HttpPost("send")]
public async Task<ActionResult<ChatMessageResponseDto>> SendMessage([FromBody] ChatMessageCreateDto messageDto)
{
    try
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine($"❌ ModelState invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
            return BadRequest(ModelState);
        }

        Console.WriteLine($"🔍 SendMessage called: '{messageDto.Message}' to {messageDto.ReceiverId}");
        
        var senderId = GetUserIdFromToken();
        Console.WriteLine($"🔍 Sender ID from token: {senderId}");

        if (messageDto.ReceiverId == Guid.Empty)
        {
            Console.WriteLine("❌ ReceiverId is empty");
            return BadRequest("ReceiverId is required");
        }

        var message = await _chatService.SendMessageAsync(messageDto, senderId);
        
        Console.WriteLine($"✅ Message sent successfully: {message.Id}");
        return Ok(message);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR in SendMessage: {ex.Message}");
        Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
        return StatusCode(500, $"Error sending message: {ex.Message}");
    }
}
        [HttpGet("conversation/{otherUserId}")]
        public async Task<ActionResult<List<ChatMessageResponseDto>>> GetConversation(Guid otherUserId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var messages = await _chatService.GetConversationAsync(userId, otherUserId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation");
                return StatusCode(500, "Error retrieving conversation");
            }
        }

      [HttpGet("conversations")]
public async Task<ActionResult<List<ChatConversationDto>>> GetConversations()
{
    try
    {
        var userId = GetUserIdFromToken();
        Console.WriteLine($"🔍 Getting REAL conversations for user: {userId}");
       
        var conversations = await _chatService.GetUserConversationsAsync(userId);
        
        if (conversations == null || !conversations.Any())
        {
            Console.WriteLine($"ℹ️ No conversations found for user {userId}, returning empty list");
            return Ok(new List<ChatConversationDto>());
        }
        
        Console.WriteLine($"✅ Returning {conversations.Count} REAL conversations");
        return Ok(conversations);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR in GetConversations: {ex.Message}");
        _logger.LogError(ex, "Error retrieving conversations for user {UserId}", GetUserIdFromToken());
        return StatusCode(500, $"Error retrieving conversations: {ex.Message}");
    }
}

        [HttpGet("unread")]
        public async Task<ActionResult<List<ChatMessageResponseDto>>> GetUnreadMessages()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var messages = await _chatService.GetUnreadMessagesAsync(userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread messages");
                return StatusCode(500, "Error retrieving unread messages");
            }
        }

        [HttpPost("conversation/{otherUserId}/read")]
        public async Task<IActionResult> MarkConversationAsRead(Guid otherUserId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                await _chatService.MarkConversationAsReadAsync(userId, otherUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation as read");
                return StatusCode(500, "Error marking conversation as read");
            }
        }

        [HttpGet("unread/count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var count = await _chatService.GetUnreadCountAsync(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread count");
                return StatusCode(500, "Error retrieving unread count");
            }
        }

        [HttpGet("debug-auth")] 
        public ActionResult DebugAuth()
        {
            try
            {
                var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
                
                Console.WriteLine("🔍 DEBUG AUTH - All claims:");
                foreach (var claim in claims)
                {
                    Console.WriteLine($"   {claim.Type}: {claim.Value}");
                }
                
                var userId = GetUserIdFromToken();
                
                return Ok(new { 
                    success = true,
                    userId = userId,
                    claims = claims
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    success = false,
                    error = ex.Message,
                    claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
                });
            }
        }

        private Guid GetUserIdFromToken()
        {
            try
            {
                Console.WriteLine("🔍 Searching for user ID in token claims...");
                
                var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                Console.WriteLine($"📋 All claims in token: {string.Join(", ", allClaims)}");
                
                var userIdClaim = 
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??  // Standard claim
                    User.FindFirst("userId")?.Value ??                   // Custom claim
                    User.FindFirst("sub")?.Value ??                      // JWT subject
                    User.FindFirst(ClaimTypes.Sid)?.Value;               // Security identifier
                
                Console.WriteLine($"🔍 UserIdClaim found: {userIdClaim ?? "NULL"}");
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    Console.WriteLine($"📧 User email from token: {userEmail}");
                    
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                     
                        var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                        if (user != null)
                        {
                            Console.WriteLine($"✅ Found user by email: {user.Id}");
                            return user.Id;
                        }
                    }
                    
                    throw new UnauthorizedAccessException("No valid user ID found in token");
                }
                
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    Console.WriteLine($"❌ Invalid user ID format: {userIdClaim}");
                    throw new UnauthorizedAccessException("Invalid user ID format in token");
                }
                
                Console.WriteLine($"✅ Parsed user ID: {userId}");
                return userId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR in GetUserIdFromToken: {ex.Message}");
                throw;
            }
        }
    }
}