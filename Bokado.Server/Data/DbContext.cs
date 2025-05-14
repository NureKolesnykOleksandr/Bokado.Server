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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123"),
                    IsAdmin = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastActive = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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