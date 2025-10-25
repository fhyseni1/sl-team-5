using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Chat;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatMessage> AddAsync(ChatMessage message);
        Task<List<ChatMessage>> GetConversationAsync(Guid user1Id, Guid user2Id);
        Task<List<ChatConversationDto>> GetUserConversationsAsync(Guid userId);
        Task<List<ChatMessage>> GetUnreadMessagesAsync(Guid userId);
        Task MarkAsReadAsync(Guid messageId);
        Task MarkConversationAsReadAsync(Guid user1Id, Guid user2Id);
        Task<ChatMessage?> GetByIdAsync(Guid id);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}