using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using ChatSystem.core.KeyConfiguration;
using System.Security.Cryptography;
using ChatSystem.DTOs;
using Superpower.Model;
using ChatSystem.Models;
namespace ChatSystem.core.Jwt;
public interface  IJwtTokenServices
{
    string CreateAccessToken(int userId);
    (string rawToken, string tokenHash) GenerateRefreshToken();
    string HashToken(string rawToken);
}
public class JwtServices : IJwtTokenServices
{
    private readonly string __JWTKeyString;
    private readonly string __IssuerKeyString;
    private readonly string __AudienceKeyString;
    public JwtServices(IOptions<JwtSettings> Keys)
    {
        __JWTKeyString = Keys.Value.Key;
        __IssuerKeyString = Keys.Value.Issuer;
        __AudienceKeyString = Keys.Value.Audience;
    }
    public string CreateAccessToken(int userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(__JWTKeyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = creds,
                Issuer = __IssuerKeyString,
                Audience = __AudienceKeyString
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
    }

    public (string rawToken, string tokenHash) GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var raw = Convert.ToBase64String(bytes);
        return (raw, HashToken(raw));
    }
    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    public RefreshToken RefreshTokenFactory(int userId, string hashToken)
        => new RefreshToken
        {
            UserId = userId,
            TokenHash = hashToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
}