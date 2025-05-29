using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Bokado.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bokado.Server.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly SocialNetworkContext _context;
        private readonly FileService _fileService;


        public ChatRepository(SocialNetworkContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<List<Chat>> GetChats(int userId)
        {
            List<int> chatIds = await _context.ChatParticipants.Where(cp => cp.UserId == userId).Select(cp => cp.ChatId).ToListAsync();
            var chats = await _context.Chats.Where(c=>chatIds.Contains(c.ChatId)).ToListAsync();
            return chats;
        }

        public async Task<List<Message>> GetMessages(int chatId)
        {
            var messages = await _context.Messages.Where(m=>m.ChatId == chatId).ToListAsync();
            return messages;
        }

        public async Task<IdentityResult> SendMessage(int fromId,MessageDto messageDto)
        {
             var sender = await _context.Users
                .Include(u => u.ChatParticipants)
                .FirstOrDefaultAsync(u => u.UserId == fromId);

            if (sender == null)
                return IdentityResult.Failed(new IdentityError { Description = "Sender not found" });

            var receiver = await _context.Users.FindAsync(messageDto.ToId);
            if (receiver == null)
                return IdentityResult.Failed(new IdentityError { Description = "Receiver not found" });

            var chat = await GetOrCreateChatAsync(sender, receiver);



            string attachmentPath = "";
            if (messageDto.attachedFile != null)
            {
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Messages");
                var allowedExtensions = new[] { ".mp3", ".gif", ".png" , ".jpg"};
                var attachmentFileName = await _fileService.SaveFileAsync(
                    messageDto.attachedFile,
                    webRootPath,
                    allowedExtensions,
                    Path.GetFileNameWithoutExtension(messageDto.attachedFile.FileName));

                if (attachmentFileName == "")
                {
                    throw new ArgumentException("File wasn`t saved");
                }

                attachmentPath = $"/Messages/{attachmentFileName}";
            }

            var message = new Message
            {
                ChatId = chat.ChatId,
                SenderId = sender.UserId,
                Text = messageDto.Text,
                SentAt = DateTime.UtcNow,
                Attachment = attachmentPath
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        private async Task<Chat> GetOrCreateChatAsync(User user1, User user2)
        {
            var existingChat = await _context.ChatParticipants
                .Where(cp1 => cp1.UserId == user1.UserId)
                .Join(_context.ChatParticipants.Where(cp2 => cp2.UserId == user2.UserId),
                    cp1 => cp1.ChatId,
                    cp2 => cp2.ChatId,
                    (cp1, cp2) => cp1.Chat)
                .FirstOrDefaultAsync();

            if (existingChat != null)
                return existingChat;

            var newChat = new Chat { CreatedAt = DateTime.UtcNow };
            _context.Chats.Add(newChat);
            await _context.SaveChangesAsync();

            _context.ChatParticipants.AddRange(
                new ChatParticipant { ChatId = newChat.ChatId, UserId = user1.UserId },
                new ChatParticipant { ChatId = newChat.ChatId, UserId = user2.UserId }
            );

            await _context.SaveChangesAsync();
            return newChat;
        }

        private async Task<string> SaveAttachmentAsync(IFormFile file, string username)
        {
            string attachmentsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "messages");
            Directory.CreateDirectory(attachmentsPath);

            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = $"{DateTime.UtcNow.Ticks}_{username}{fileExtension}";
            string filePath = Path.Combine(attachmentsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/messages/{fileName}";
        }

    }
}
