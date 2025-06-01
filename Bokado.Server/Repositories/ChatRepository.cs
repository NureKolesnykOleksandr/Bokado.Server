using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Bokado.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;

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

        public async Task<ChatDto> CreateChat(int fromId, int toId)
        {
            var user1 = await _context.Users.Where(u => u.UserId == fromId).FirstOrDefaultAsync();
            var user2 = await _context.Users.Where(u => u.UserId == toId).FirstOrDefaultAsync();

            if (user1 == null || user2 == null)
            {
                throw new KeyNotFoundException("One user was not found");
            }

            var existingChat = await _context.ChatParticipants
                .Where(cp1 => cp1.UserId == user1.UserId)
                .Join(_context.ChatParticipants.Where(cp2 => cp2.UserId == user2.UserId),
                    cp1 => cp1.ChatId,
                    cp2 => cp2.ChatId,
                    (cp1, cp2) => new ChatDto
                    {
                        ChatId = cp1.Chat.ChatId,
                        CreatedAt = cp1.Chat.CreatedAt,
                        SecondMember = new UserDto
                        {
                            UserId = user2.UserId,
                            Username = user2.Username,
                            Email = user2.Email,
                            IsAdmin = user2.IsAdmin
                        }
                    })
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

            return new ChatDto
            {
                ChatId = newChat.ChatId,
                CreatedAt = newChat.CreatedAt,
                SecondMember = new UserDto
                {
                    UserId = user2.UserId,
                    Username = user2.Username,
                    Email = user2.Email,
                    IsAdmin = user2.IsAdmin
                }
            };
        }

        public async Task<List<ChatDto>> GetChats(int userId)
        {
            return await _context.ChatParticipants
                .Where(cp => cp.UserId == userId)
                .Select(cp => new ChatDto
                {
                    ChatId = cp.Chat.ChatId,
                    CreatedAt = cp.Chat.CreatedAt,
                    SecondMember = cp.Chat.Participants
                        .Where(p => p.UserId != userId)
                        .Select(p => new UserDto
                        {
                            UserId = p.User.UserId,
                            Username = p.User.Username,
                            Email = p.User.Email,
                            IsAdmin = p.User.IsAdmin
                        }).First()
                })
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessages(int chatId)
        {
            var messages = await _context.Messages.Where(m=>m.ChatId == chatId).Include(m=>m.Sender).ToListAsync();
            return messages;
        }

        public async Task<IdentityResult> SendMessage(int fromId,MessageDto messageDto)
        {   
             var sender = await _context.Users
                .Include(u => u.ChatParticipants)
                .FirstOrDefaultAsync(u => u.UserId == fromId);

            if (sender == null)
                return IdentityResult.Failed(new IdentityError { Description = "Sender not found" });

            var chat = await _context.Chats.Where(c => c.ChatId == messageDto.ChatId).FirstOrDefaultAsync();
            if(chat == null)
            {
                throw new KeyNotFoundException("Chat was not found");
            }

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
