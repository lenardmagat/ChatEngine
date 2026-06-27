using BCryptTool = BCrypt.Net.BCrypt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using HashidsNet;
using System.Security.Authentication;
using ChatSystem.ErrorHandling;
public interface IHasher
{
    public string HashPassword(string password);
    public bool VerifyPassword(string password, string hashPassword);
    public string CreateToken(int UserId);
    public string CreateHashids(int GroupId);
    public Result<int> DecodeHashids(string hashh);
}
namespace ChatSystem.core  
{
    public class SystemSecurity : IHasher
    {
        private readonly IHashids _hashids;
        private readonly string __JWTKeyString;
        private readonly string __IssuerKeyString;
        private readonly string __AudienceKeyString;
        public SystemSecurity(IHashids hashids, string keyString, string issuer, string audience)
        {
            _hashids = hashids;
            __JWTKeyString = keyString;
            __IssuerKeyString = issuer;
            __AudienceKeyString = audience;
        }
    public string HashPassword(string password)
        => BCryptTool.HashPassword(password, workFactor: 12);
    public bool VerifyPassword(string password, string hashPassword)
        => BCryptTool.Verify(password, hashPassword);
    
    public string CreateToken(int Userid)
        {
            DotNetEnv.Env.Load();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(__JWTKeyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Userid.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2), // Ticket lasts 2 hours
                SigningCredentials = creds,
                Issuer = __IssuerKeyString,
                Audience = __AudienceKeyString
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    public string CreateHashids(int GroupId)
        {
            return _hashids.Encode(GroupId);
        }
    public Result<int> DecodeHashids(string hash)
        {
            if(string.IsNullOrWhiteSpace(hash)) 
                return Result<int>.Failure("Provided ID is empty.", 406);

            if(!_hashids.TryDecodeSingle(hash, out int decoded) || decoded == 0) 
                return Result<int>.Failure("Invalid Id or Corrupted Id format", 403);
            return Result<int>.Success(decoded);
        }
        
    }
}

