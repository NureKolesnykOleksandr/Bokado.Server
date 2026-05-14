using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bokado.Server.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Data
{
    public class SocialNetworkContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<UserChallenge> UserChallenges { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupInterest> GroupInterests { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }


        public SocialNetworkContext(DbContextOptions<SocialNetworkContext> options): base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=12345;Database=BokadoDB;Port=5432");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(u => u.UserId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<Interest>().Property(i => i.InterestId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<FriendRequest>().Property(fr => fr.FriendRequestId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<Chat>().Property(c => c.ChatId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<Message>().Property(m => m.MessageId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<Event>().Property(e => e.EventId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<Challenge>().Property(c => c.ChallengeId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<UserInterest>().Property(ui => ui.UserInterestId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<Friendship>().Property(f => f.FriendshipId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<ChatParticipant>().Property(cp => cp.ChatParticipantId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<EventParticipant>().Property(ep => ep.EventParticipantId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<UserChallenge>().Property(uc => uc.UserChallengeId).UseIdentityAlwaysColumn();



            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = "$2a$11$QVlAssqGxFq2bvc4kKYI4O/K/bD5UQpcClAbDS/f9W81551lIEgri",
                    IsAdmin = true,
                    BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastActive = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<Interest>().HasData(
                new Interest { InterestId = 1, Name = "Ігри" },
                new Interest { InterestId = 2, Name = "Музика" },
                new Interest { InterestId = 3, Name = "Кіно" },
                new Interest { InterestId = 4, Name = "Спорт" },
                new Interest { InterestId = 5, Name = "Подорожі" },
                new Interest { InterestId = 6, Name = "Кулінарія" },
                new Interest { InterestId = 7, Name = "Мистецтво" },
                new Interest { InterestId = 8, Name = "Технології" },
                new Interest { InterestId = 9, Name = "Книги" },
                new Interest { InterestId = 10, Name = "Фітнес" },
                new Interest { InterestId = 11, Name = "Фотографія" },
                new Interest { InterestId = 12, Name = "Природа" }
            );

            modelBuilder.Entity<Challenge>().HasData(
                new Challenge
                {
                    ChallengeId = 1,
                    Title = "Соціальний старт",
                    Description = "Почни спілкуватися з 3 різними людьми",
                    Reward = 2,
                    CreatedAt = new DateTime(2025,5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 2,
                    Title = "Активний учасник",
                    Description = "Відправ 10 повідомлень у чатах",
                    Reward = 2,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 3,
                    Title = "Знайомство з подією",
                    Description = "Прийми участь у будь-якій події",
                    Reward = 1,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 4,
                    Title = "Дружелюбний крок",
                    Description = "Додай 5 нових людей у друзі",
                    Reward = 2,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 5,
                    Title = "Чат-ентузіаст",
                    Description = "Підтримуй бесіду в 3 різних чатах",
                    Reward = 1,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 6,
                    Title = "Організатор",
                    Description = "Створи власну подію",
                    Reward = 3,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 7,
                    Title = "Дослідник",
                    Description = "Знайди 7 людей зі спільними інтересами",
                    Reward = 2,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 8,
                    Title = "Віртуальний зустріч",
                    Description = "Відправ хоча б одне голосове повідомлення",
                    Reward = 1,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 9,
                    Title = "Соціальний активіст",
                    Description = "Відвідай 2 різні події за тиждень",
                    Reward = 3,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 10,
                    Title = "Профільний експерт",
                    Description = "Заповни всі поля свого профілю на 100%",
                    Reward = 1,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 11,
                    Title = "Перший пост",
                    Description = "Опублікуй свій перший пост",
                    Reward = 1,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 12,
                    Title = "Популярний контент",
                    Description = "Отримай 5 лайків на один пост",
                    Reward = 2,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 13,
                    Title = "Груповий учасник",
                    Description = "Вступи в свою першу групу",
                    Reward = 1,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 14,
                    Title = "Лідер спільноти",
                    Description = "Створи власну групу",
                    Reward = 3,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 15,
                    Title = "Фотограф",
                    Description = "Надішли фото у повідомленні",
                    Reward = 1,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 16,
                    Title = "Відеоорганізатор",
                    Description = "Запусти відеодзвінок у групі",
                    Reward = 2,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 17,
                    Title = "Магніт",
                    Description = "Отримай 3 запити на дружбу",
                    Reward = 2,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 18,
                    Title = "Відданий друг",
                    Description = "Набери 10 друзів",
                    Reward = 3,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 19,
                    Title = "Активний лайкер",
                    Description = "Постав 10 лайків на пости інших",
                    Reward = 1,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                },
                new Challenge
                {
                    ChallengeId = 20,
                    Title = "Командний гравець",
                    Description = "Вступи в 3 різні групи",
                    Reward = 3,
                    CreatedAt = new DateTime(2025, 5, 21, 20, 30, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserInterests)
                .HasForeignKey(ui => ui.UserId);

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.Interest)
                .WithMany(i => i.UserInterests)
                .HasForeignKey(ui => ui.InterestId);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.FromUser)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(fr => fr.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.ToUser)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(fr => fr.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.FromUserId, fr.ToUserId })
                .IsUnique();

            modelBuilder.Entity<ChatParticipant>()
                .HasOne(cp => cp.Chat)
                .WithMany(c => c.Participants)
                .HasForeignKey(cp => cp.ChatId);

            modelBuilder.Entity<ChatParticipant>()
                .HasOne(cp => cp.User)
                .WithMany(u => u.ChatParticipants)
                .HasForeignKey(cp => cp.UserId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(e => e.CreatorId);

            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.EventId);

            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.User)
                .WithMany(u => u.EventParticipants)
                .HasForeignKey(ep => ep.UserId);

            modelBuilder.Entity<UserChallenge>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserChallenges)
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserChallenge>()
                .HasOne(uc => uc.Challenge)
                .WithMany(c => c.UserChallenges)
                .HasForeignKey(uc => uc.ChallengeId);

            modelBuilder.Entity<Group>().Property(g => g.GroupId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<GroupMember>().Property(gm => gm.GroupMemberId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<GroupInterest>().Property(gi => gi.GroupInterestId).UseIdentityAlwaysColumn();

            modelBuilder.Entity<Group>()
                .HasOne(g => g.Creator)
                .WithMany()
                .HasForeignKey(g => g.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.Chat)
                .WithMany()
                .HasForeignKey(g => g.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany()
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupInterest>()
                .HasOne(gi => gi.Group)
                .WithMany(g => g.GroupInterests)
                .HasForeignKey(gi => gi.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupInterest>()
                .HasOne(gi => gi.Interest)
                .WithMany()
                .HasForeignKey(gi => gi.InterestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>().Property(p => p.PostId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<PostLike>().Property(pl => pl.PostLikeId).UseIdentityAlwaysColumn();

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany()
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.PostId, pl.UserId })
                .IsUnique();
        }
    }
}