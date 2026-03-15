using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CheckerBA.Application.DTOs;
using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CheckerBA.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest req)
        {
            var user = await _userRepo.GetByUsernameAsync(req.Username);
            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return null;

            return BuildToken(user);
        }

        public async Task<LoginResponse?> RegisterAsync(RegisterRequest req)
        {
            // Username không được trùng
            var existing = await _userRepo.GetByUsernameAsync(req.Username);
            if (existing is not null) return null;

            var validRoles = new[] { "Admin", "Operator" };
            if (!validRoles.Contains(req.Role)) return null;

            var user = new AppUser
            {
                Username = req.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
            return BuildToken(user);
        }

        private LoginResponse BuildToken(AppUser user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "60");
            var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                },
                expires: expiresAt,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new LoginResponse(
                new JwtSecurityTokenHandler().WriteToken(token),
                user.Username,
                user.Role,
                expiresAt);
        }
    }
}
