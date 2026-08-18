using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.DTOs;
using backend.Repository;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly UserRepository _userRepo;

    public AuthService(IConfiguration config, UserRepository userRepo)
    {
        _config = config;
        _userRepo = userRepo;
    }

    public async Task<LoginResponseDto?> AuthenticateAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email);
        if (user is null)
            return null;

        var hash = HashPassword(dto.Password, user.PasswordSalt);
        if (hash != user.PasswordHash)
            return null;

        var expiresAt = DateTime.UtcNow.AddHours(8);
        var token = GenerateToken(user.Email, user.Name, expiresAt);

        return new LoginResponseDto
        {
            Email = user.Email,
            Name = user.Name,
            ExpiresAt = expiresAt,
        };
    }

    public TokenValidationParameters GetValidationParameters()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSecretKey()));
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = "repositorycontrol",
            ValidateAudience = true,
            ValidAudience = "repositorycontrol",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    }

    public string GenerateToken(string email, string name, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSecretKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: "repositorycontrol",
            audience: "repositorycontrol",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password, string salt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToBase64String(bytes);
    }

    private string GetSecretKey() =>
        _config["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey not configured.");
}
