using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IChatRepository
    {
        public Task<List<ChatDto>> GetChats(int userId);
        public Task<List<Message>> GetMessages(int chatId);
        public Task<IdentityResult> SendMessage(int fromId, MessageDto messageDto);
        public Task<ChatDto> CreateChat(int fromId, int toId);
        public Task<IdentityResult> DeleteMessage(int userId, int messageId);

    }
}
