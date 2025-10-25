using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Chat;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ChatService(
            IChatRepository chatRepository, 
            IUserRepository userRepository,
            IMapper mapper)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ChatMessageResponseDto> SendMessageAsync(ChatMessageCreateDto messageDto, Guid senderId)
        {
            try
            {
                Console.WriteLine($"🔍 SendMessageAsync: Sender={senderId}, Receiver={messageDto.ReceiverId}, Message={messageDto.Message}");
                var sender = await _userRepository.GetByIdAsync(senderId);
                var receiver = await _userRepository.GetByIdAsync(messageDto.ReceiverId);

                if (sender == null)
                    throw new Exception($"Sender with ID '{senderId}' not found");
                if (receiver == null)
                    throw new Exception($"Receiver with ID '{messageDto.ReceiverId}' not found");

                var message = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    SenderId = senderId,
                    ReceiverId = messageDto.ReceiverId,
                    Message = messageDto.Message?.Trim() ?? string.Empty,
                    ParentMessageId = messageDto.ParentMessageId,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };
                var savedMessage = await _chatRepository.AddAsync(message);
                Console.WriteLine($"✅ Message saved to database: {savedMessage.Id}");
                var responseDto = new ChatMessageResponseDto
                {
                    Id = savedMessage.Id,
                    SenderId = savedMessage.SenderId,
                    SenderName = $"{sender.FirstName} {sender.LastName}",
                    ReceiverId = savedMessage.ReceiverId,
                    ReceiverName = $"{receiver.FirstName} {receiver.LastName}",
                    Message = savedMessage.Message ?? string.Empty,
                    IsRead = savedMessage.IsRead,
                    SentAt = savedMessage.SentAt
                };

                return responseDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in SendMessageAsync: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
        public async Task<List<ChatMessageResponseDto>> GetConversationAsync(Guid user1Id, Guid user2Id)
        {
            try
            {
                var messages = await _chatRepository.GetConversationAsync(user1Id, user2Id);
                var dtos = new List<ChatMessageResponseDto>();
                var mainMessages = messages.Where(m => m.ParentMessageId == null);

                foreach (var message in mainMessages)
                {
                    var dto = await MapToDto(message);
                    dtos.Add(dto);
                }

                return dtos.OrderBy(m => m.SentAt).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetConversationAsync: {ex.Message}");
                return new List<ChatMessageResponseDto>();
            }
        }

        public async Task<List<ChatConversationDto>> GetUserConversationsAsync(Guid userId)
        {
            try
            {
                Console.WriteLine($"🔍 ChatService: Getting REAL conversations for user {userId}");
                var conversations = await _chatRepository.GetUserConversationsAsync(userId);
                
                if (!conversations.Any())
                {
                    Console.WriteLine($"ℹ️ No conversations found for user {userId}");
                }
                
                Console.WriteLine($"✅ ChatService: Returning {conversations.Count} REAL conversations from repository");
                return conversations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in ChatService.GetUserConversationsAsync: {ex.Message}");
                return new List<ChatConversationDto>();
            }
        }

        public async Task<List<ChatMessageResponseDto>> GetUnreadMessagesAsync(Guid userId)
        {
            try
            {
                var messages = await _chatRepository.GetUnreadMessagesAsync(userId);
                var dtos = new List<ChatMessageResponseDto>();

                foreach (var message in messages)
                {
                    var dto = await MapToDto(message);
                    dtos.Add(dto);
                }

                return dtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetUnreadMessagesAsync: {ex.Message}");
                return new List<ChatMessageResponseDto>();
            }
        }

        public async Task MarkConversationAsReadAsync(Guid user1Id, Guid user2Id)
        {
            try
            {
                await _chatRepository.MarkConversationAsReadAsync(user1Id, user2Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in MarkConversationAsReadAsync: {ex.Message}");
            }
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                return await _chatRepository.GetUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetUnreadCountAsync: {ex.Message}");
                return 0;
            }
        }

        private async Task<ChatMessageResponseDto> MapToDto(ChatMessage message)
        {
            try
            {
                var sender = await _userRepository.GetByIdAsync(message.SenderId);
                var receiver = await _userRepository.GetByIdAsync(message.ReceiverId);

                var dto = new ChatMessageResponseDto
                {
                    Id = message.Id,
                    SenderId = message.SenderId,
                    SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "Unknown User",
                    ReceiverId = message.ReceiverId,
                    ReceiverName = receiver != null ? $"{receiver.FirstName} {receiver.LastName}" : "Unknown User",
                    Message = message.Message,
                    IsRead = message.IsRead,
                    ParentMessageId = message.ParentMessageId,
                    ParentMessage = message.ParentMessage?.Message,
                    SentAt = message.SentAt
                };

                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in MapToDto: {ex.Message}");
                return new ChatMessageResponseDto
                {
                    Id = message.Id,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    Message = message.Message,
                    IsRead = message.IsRead,
                    SentAt = message.SentAt
                };
            }
        }
    }
}