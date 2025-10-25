using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Chat;

namespace UserHealthService.Application.Interfaces
{
    public interface IChatService
    {
        Task<ChatMessageResponseDto> SendMessageAsync(ChatMessageCreateDto messageDto, Guid senderId);
        Task<List<ChatMessageResponseDto>> GetConversationAsync(Guid user1Id, Guid user2Id);
        Task<List<ChatConversationDto>> GetUserConversationsAsync(Guid userId);
        Task<List<ChatMessageResponseDto>> GetUnreadMessagesAsync(Guid userId);
        Task MarkConversationAsReadAsync(Guid user1Id, Guid user2Id);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}