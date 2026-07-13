using BCryptTool = BCrypt.Net.BCrypt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using HashidsNet;
using Microsoft.Extensions.Options;
using ChatSystem.ErrorHandling;
using ChatSystem.core.KeyConfiguration;
namespace ChatSystem.core
{
public enum HashContext
{
    User,
    Room,
    Message,
    Participant
}
public interface IHasher
{
    public string HashPassword(string password);
    public bool VerifyPassword(string password, string hashPassword);
    public string CreateToken(int UserId);
    public string CreateHashids(int GroupId, HashContext hashContext);
    public Result<int> DecodeHashids(string hashh, HashContext hashContext);
}
    public class SystemSecurity : IHasher
    {
        private readonly IHashids _UserHashids;
        private readonly IHashids _MessageHashids;
        private readonly IHashids _RoomHashids;
        private readonly IHashids _ParticipantHahids;
        private readonly string __JWTKeyString;
        private readonly string __IssuerKeyString;
        private readonly string __AudienceKeyString;
        public SystemSecurity(IOptions<HashidsSettings> hashidsOptions, IOptions<JwtSettings> jwtOptions)
        {
            __JWTKeyString = jwtOptions.Value.Key;
            __IssuerKeyString = jwtOptions.Value.Issuer;
            __AudienceKeyString = jwtOptions.Value.Audience;

            _UserHashids = new Hashids($"{hashidsOptions.Value.HasherSalt}_UsersContext", hashidsOptions.Value.MinHashLength);
            _RoomHashids = new Hashids($"{hashidsOptions.Value.HasherSalt}_RoomsContext", hashidsOptions.Value.MinHashLength);
            _ParticipantHahids = new Hashids($"{hashidsOptions.Value.HasherSalt}_ParticipantContext", hashidsOptions.Value.MinHashLength);
            _MessageHashids = new Hashids($"{hashidsOptions.Value.HasherSalt}_MessageContext", hashidsOptions.Value.MinHashLength);
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
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = creds,
                Issuer = __IssuerKeyString,
                Audience = __AudienceKeyString
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    public string CreateHashids(int Id, HashContext hashContext) => hashContext switch
    {
        HashContext.User => _UserHashids.Encode(Id),
        HashContext.Room => _RoomHashids.Encode(Id),
        HashContext.Message => _MessageHashids.Encode(Id),
        HashContext.Participant => _ParticipantHahids.Encode(Id),
        _ => _UserHashids.Encode(Id)
    };
    public Result<int> DecodeHashids(string hash, HashContext hashContext)
        {
            if(string.IsNullOrWhiteSpace(hash)) 
                return Result<int>.Failure("Provided ID is empty.", 406);

            var hashidInstance = hashContext switch
            {
                HashContext.User => _UserHashids,
                HashContext.Room => _RoomHashids,
                HashContext.Message => _MessageHashids,
                HashContext.Participant => _ParticipantHahids,
                _ => _UserHashids
            };
            if(!hashidInstance.TryDecodeSingle(hash, out int decoded) || decoded == 0) 
                return Result<int>.Failure("Invalid Id or Corrupted Id format", 403);
            return Result<int>.Success(decoded);
        }
        
    }
}

