﻿// Repositories/AuthRepository.cs
using System.Security.Claims;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.EntityFrameworkCore;
using Bokado.Server.Dtos;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Bokado.Server.Data;
using Bokado.Server.Services;
using System.Xml.Linq;

namespace Bokado.Server.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly SocialNetworkContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthRepository(SocialNetworkContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        private static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<AuthResultDTO> Register(RegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new ArgumentException("Email already exists");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                BirthDate = dto.BirthDate
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            int userId = await _context.Users.Where(u => u.Email == user.Email).Select(u=>u.UserId).FirstOrDefaultAsync();

            return new AuthResultDTO
            {
                Token = GenerateJwtToken(user),
                User = new UserDto() { Email = user.Email, IsAdmin = false, UserId = userId, Username = user.Username}
            };
        }

        public async Task<AuthResultDTO> Login(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            if (user.IsBanned && !user.IsAdmin)
            {
                throw new UnauthorizedAccessException("U SHALL NOT PASS!(u are banned, sorry 😢)");
            }

            user.LastActive = DateTime.UtcNow;

            return new AuthResultDTO
            {
                Token = GenerateJwtToken(user),
                User = new UserDto() { Email = user.Email, IsAdmin = user.IsAdmin, UserId = user.UserId, Username = user.Username }
            };
        }

        public async Task<IdentityResult> ResetPassword(string email)
        {
            User? user = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User was not found"});
            }

            string password = GenerateRandomPassword();

            string text = $"Hi dear {user.Username}. " +
                $"Your password was reset to {password}. " +
                $"Have a nice day.";
            try
            {
                bool emailSended = await _emailService.SendEmailAsync(email, "Password Reset", text, user.Username);
                if (!emailSended)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Не вдалося відправити повідомлення" });
                }
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Щось пішло не так" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}