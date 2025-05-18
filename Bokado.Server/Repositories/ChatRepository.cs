using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Repositories
{
    public class ChatRepository : IChatRepository
    {
        public Task<List<Chat>> GetChats(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Message>> GetMessages(int chatId)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> SendMessage(int fromUserId, int chatId, string message)
        {
            throw new NotImplementedException();
        }
    }
}
