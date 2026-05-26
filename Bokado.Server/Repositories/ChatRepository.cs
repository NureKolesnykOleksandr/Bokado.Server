using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Bokado.Server.Services;
using Microsoft.AspNetCore.Identity;
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

        public async Task<List<ChatDto>> GetChats(int userId)
        {
            var result = new List<ChatDto>();

            var personalChats = await _context.ChatParticipants
                .Where(cp => cp.UserId == userId)
                .Where(cp => !_context.Groups.Any(g => g.ChatId == cp.ChatId))
                .Include(cp => cp.Chat)
                    .ThenInclude(c => c.Participants)
                        .ThenInclude(p => p.User)
                .Include(cp => cp.Chat)
                    .ThenInclude(c => c.Messages)
                .ToListAsync();

            foreach (var cp in personalChats)
            {
                var chat = cp.Chat;
                var secondMemberParticipant = chat.Participants.FirstOrDefault(p => p.UserId != userId);
                if (secondMemberParticipant == null) continue;

                var secondUser = secondMemberParticipant.User;
                var lastMsg = chat.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unread = chat.Messages.Count(m => !m.IsRead && m.SenderId != userId);

                result.Add(new ChatDto
                {
                    ChatId = chat.ChatId,
                    CreatedAt = chat.CreatedAt,
                    IsGroup = false,
                    SecondMember = new ChatMemberDto
                    {
                        UserId = secondUser.UserId,
                        Username = secondUser.Username,
                        Email = secondUser.Email,
                        IsAdmin = secondUser.IsAdmin,
                        AvatarUrl = secondUser.AvatarUrl
                    },
                    LastMessage = lastMsg == null ? null : new LastMessageDto
                    {
                        MessageId = lastMsg.MessageId,
                        Text = lastMsg.Text ?? "",
                        Attachment = lastMsg.Attachment,
                        SentAt = lastMsg.SentAt,
                        SenderId = lastMsg.SenderId,
                        IsRead = lastMsg.IsRead
                    },
                    UnreadCount = unread
                });
            }

            var groupMemberships = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                    .ThenInclude(g => g.Chat)
                        .ThenInclude(c => c.Messages)
                .ToListAsync();

            foreach (var gm in groupMemberships)
            {
                var group = gm.Group;
                if (group?.Chat == null) continue;

                var chat = group.Chat;
                var lastMsg = chat.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unread = chat.Messages.Count(m => !m.IsRead && m.SenderId != userId);

                result.Add(new ChatDto
                {
                    ChatId = chat.ChatId,
                    CreatedAt = chat.CreatedAt,
                    IsGroup = true,
                    GroupId = group.GroupId,
                    GroupName = group.Name,
                    LastMessage = lastMsg == null ? null : new LastMessageDto
                    {
                        MessageId = lastMsg.MessageId,
                        Text = lastMsg.Text ?? "",
                        Attachment = lastMsg.Attachment,
                        SentAt = lastMsg.SentAt,
                        SenderId = lastMsg.SenderId,
                        IsRead = lastMsg.IsRead
                    },
                    UnreadCount = unread
                });
            }

            return result.OrderByDescending(c => c.LastMessage?.SentAt ?? c.CreatedAt).ToList();
        }

        public async Task MarkChatAsRead(int chatId, int userId)
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.ChatId == chatId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unreadMessages)
                msg.IsRead = true;

            if (unreadMessages.Any())
                await _context.SaveChangesAsync();
        }

        public async Task<List<Message>> GetMessages(int chatId)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<ChatDto> CreateChat(int fromId, int toId)
        {
            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.UserId == fromId);
            var user2 = await _context.Users.FirstOrDefaultAsync(u => u.UserId == toId);

            if (user1 == null || user2 == null)
                throw new KeyNotFoundException("One user was not found");

            var existingChatId = await _context.ChatParticipants
                .Where(cp1 => cp1.UserId == fromId)
                .Join(_context.ChatParticipants.Where(cp2 => cp2.UserId == toId),
                    cp1 => cp1.ChatId, cp2 => cp2.ChatId, (cp1, cp2) => (int?)cp1.ChatId)
                .Where(cid => !_context.Groups.Any(g => g.ChatId == cid))
                .FirstOrDefaultAsync();

            if (existingChatId != null)
            {
                return new ChatDto
                {
                    ChatId = existingChatId.Value,
                    IsGroup = false,
                    SecondMember = new ChatMemberDto
                    {
                        UserId = user2.UserId,
                        Username = user2.Username,
                        Email = user2.Email,
                        IsAdmin = user2.IsAdmin,
                        AvatarUrl = user2.AvatarUrl
                    }
                };
            }

            var newChat = new Chat { CreatedAt = DateTime.UtcNow };
            _context.Chats.Add(newChat);
            await _context.SaveChangesAsync();

            _context.ChatParticipants.AddRange(
                new ChatParticipant { ChatId = newChat.ChatId, UserId = fromId },
                new ChatParticipant { ChatId = newChat.ChatId, UserId = toId }
            );
            await _context.SaveChangesAsync();

            return new ChatDto
            {
                ChatId = newChat.ChatId,
                CreatedAt = newChat.CreatedAt,
                IsGroup = false,
                SecondMember = new ChatMemberDto
                {
                    UserId = user2.UserId,
                    Username = user2.Username,
                    Email = user2.Email,
                    IsAdmin = user2.IsAdmin,
                    AvatarUrl = user2.AvatarUrl
                }
            };
        }

        public async Task<IdentityResult> DeleteChat(int userId, int chatId)
        {
            bool isAdmin = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.IsAdmin)
                .FirstOrDefaultAsync();

            var participant = await _context.ChatParticipants
                .Where(cp => cp.UserId == userId && cp.ChatId == chatId)
                .Include(cp => cp.Chat)
                .FirstOrDefaultAsync();

            if (participant == null && !isAdmin)
                return IdentityResult.Failed(new IdentityError { Description = "No permission" });

            var chat = participant?.Chat ?? await _context.Chats.FindAsync(chatId);
            if (chat == null)
                return IdentityResult.Failed(new IdentityError { Description = "Chat not found" });

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteMessage(int userId, int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return IdentityResult.Failed(new IdentityError { Description = "Повідомлення не знайдено" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено" });

            if (user.IsAdmin || message.SenderId == userId)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                return IdentityResult.Success;
            }

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.ChatId == message.ChatId);
            if (group != null)
            {
                var member = await _context.GroupMembers
                    .FirstOrDefaultAsync(m => m.GroupId == group.GroupId && m.UserId == userId);
                if (member != null && (member.Role == GroupMemberRole.Admin || member.Role == GroupMemberRole.Owner))
                {
                    _context.Messages.Remove(message);
                    await _context.SaveChangesAsync();
                    return IdentityResult.Success;
                }
            }

            return IdentityResult.Failed(new IdentityError { Description = "Немає прав для видалення" });
        }

        public async Task<IdentityResult> SendMessage(int fromId, MessageDto messageDto)
        {
            var sender = await _context.Users.FindAsync(fromId);
            if (sender == null)
                return IdentityResult.Failed(new IdentityError { Description = "Sender not found" });

            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.ChatId == messageDto.ChatId);
            if (chat == null)
                throw new KeyNotFoundException("Chat was not found");

            string attachmentPath = "";
            if (messageDto.attachedFile != null)
            {
                var allowedExtensions = new[] { ".mp3", ".gif", ".png", ".jpg", ".jpeg", ".webp" };
                attachmentPath = await _fileService.SaveFileAsync(
                    messageDto.attachedFile, "bokado/messages", allowedExtensions,
                    Path.GetFileNameWithoutExtension(messageDto.attachedFile.FileName));

                if (attachmentPath == "")
                    throw new ArgumentException("File wasn't saved");
            }

            var message = new Message
            {
                ChatId = chat.ChatId,
                SenderId = sender.UserId,
                Text = messageDto.Text ?? "",
                SentAt = DateTime.UtcNow,
                Attachment = attachmentPath,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}
