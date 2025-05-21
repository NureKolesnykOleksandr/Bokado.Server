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
        public DbSet<Swipe> Swipes { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<UserChallenge> UserChallenges { get; set; }


        public SocialNetworkContext(DbContextOptions<SocialNetworkContext> options): base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=12345;Database=BokadoDB;Port=5432");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(u => u.UserId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<Interest>().Property(i => i.InterestId).UseIdentityAlwaysColumn();
            modelBuilder.Entity<Swipe>().Property(s => s.SwipeId).UseIdentityAlwaysColumn();
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
                    PasswordHash = ("$2a$11$QVlAssqGxFq2bvc4kKYI4O/K/bD5UQpcClAbDS/f9W81551lIEgri"),
                    IsAdmin = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastActive = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
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

            modelBuilder.Entity<Swipe>()
                .HasOne(s => s.Swiper)
                .WithMany(u => u.Swipes)
                .HasForeignKey(s => s.SwiperId);

            modelBuilder.Entity<Swipe>()
                .HasOne(s => s.TargetUser)
                .WithMany()
                .HasForeignKey(s => s.TargetUserId);

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
        }
    }
}