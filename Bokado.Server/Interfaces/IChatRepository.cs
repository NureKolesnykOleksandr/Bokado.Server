using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IChatRepository
    {
        Task<List<ChatDto>> GetChats(int userId);
        Task<List<Message>> GetMessages(int chatId);
        Task<IdentityResult> SendMessage(int fromId, MessageDto messageDto);
        Task<ChatDto> CreateChat(int fromId, int toId);
        Task<IdentityResult> DeleteMessage(int userId, int messageId);
        Task<IdentityResult> DeleteChat(int userId, int chatId);
        Task MarkChatAsRead(int chatId, int userId);
    }
}
