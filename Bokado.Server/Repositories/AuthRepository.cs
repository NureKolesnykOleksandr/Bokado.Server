// Repositories/AuthRepository.cs
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

namespace Bokado.Server.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly SocialNetworkContext _context;
        private readonly IConfiguration _config;

        public AuthRepository(SocialNetworkContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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
                User = new UserDto() { Email = user.Email, IsAdmin = false, PasswordHash = user.PasswordHash, UserId = userId, Username = user.Username}
            };
        }

        public async Task<AuthResultDTO> Login(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            return new AuthResultDTO
            {
                Token = GenerateJwtToken(user),
                User = new UserDto() { Email = user.Email, IsAdmin = user.IsAdmin, PasswordHash = user.PasswordHash, UserId = user.UserId, Username = user.Username }
            };
        }

        public async Task<bool> ResetPassword(string email)
        {
            return true;
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