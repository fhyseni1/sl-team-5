using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Chat;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly UserHealthDbContext _context;

        public ChatRepository(UserHealthDbContext context)
        {
            _context = context;
        }
public async Task<ChatMessage> AddAsync(ChatMessage message)
{
    try
    {
        Console.WriteLine($"🟡 Adding message to database: {message.Id}");
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        Console.WriteLine($"✅ Message added successfully: {message.Id}");
        return message;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR adding message to database: {ex.Message}");
        throw;
    }
}

        public async Task<List<ChatMessage>> GetConversationAsync(Guid user1Id, Guid user2Id)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ParentMessage)
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                           (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

       public async Task<List<ChatConversationDto>> GetUserConversationsAsync(Guid userId)
{
    try
    {
        Console.WriteLine($"🔍 Getting REAL conversations from database for user: {userId}");

        var conversationPartners = await _context.ChatMessages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Distinct()
            .ToListAsync();

        Console.WriteLine($"📞 Found {conversationPartners.Count} conversation partners");

        var conversations = new List<ChatConversationDto>();

        foreach (var partnerId in conversationPartners)
        {
            var lastMessage = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId && m.ReceiverId == partnerId) ||
                           (m.SenderId == partnerId && m.ReceiverId == userId))
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            if (lastMessage != null)
            {
                var unreadCount = await _context.ChatMessages
                    .CountAsync(m => m.SenderId == partnerId && 
                                   m.ReceiverId == userId && 
                                   !m.IsRead);

                var isDoctor = lastMessage.SenderId == userId 
                    ? lastMessage.Receiver.Type == Domain.Enums.UserType.HealthcareProvider
                    : lastMessage.Sender.Type == Domain.Enums.UserType.HealthcareProvider;

                var partner = lastMessage.SenderId == userId ? lastMessage.Receiver : lastMessage.Sender;
                var partnerName = $"{partner.FirstName} {partner.LastName}";

                conversations.Add(new ChatConversationDto
                {
                    OtherUserId = partnerId,
                    OtherUserName = partnerName,
                    LastMessage = lastMessage.Message.Length > 50 
                        ? lastMessage.Message.Substring(0, 50) + "..." 
                        : lastMessage.Message,
                    LastMessageTime = lastMessage.SentAt,
                    UnreadCount = unreadCount,
                    IsDoctor = isDoctor
                });
            }
        }

        var sortedConversations = conversations
            .OrderByDescending(c => c.LastMessageTime)
            .ToList();

        Console.WriteLine($"✅ Returning {sortedConversations.Count} REAL conversations from database");
        return sortedConversations;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR in GetUserConversationsAsync: {ex.Message}");
        Console.WriteLine($"📝 Stack trace: {ex.StackTrace}");
        return new List<ChatConversationDto>();
    }
}

        public async Task<List<ChatMessage>> GetUnreadMessagesAsync(Guid userId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkConversationAsReadAsync(Guid user1Id, Guid user2Id)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ReceiverId == user1Id && m.SenderId == user2Id && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ChatMessage?> GetByIdAsync(Guid id)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ParentMessage)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }
    }
}