using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IChatRepository
    {
        public Task<List<Chat>> GetChats(int userId);
        public Task<List<Message>> GetMessages(int chatId);
        public Task<IdentityResult> SendMessage(MessageDto messageDto);

    }
}
