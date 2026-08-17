using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.core.Jwt;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Services.Auth.Jwt;
public interface IAuthServices{
    public Task<Result<AuthJWTResponse>> IssueTokenAsync(int userId);
    public Task<Result<AuthJWTResponse>> RefreshTokenAsync(string presentedRawToken);
}
public class JWTAuthServices : IAuthServices
{
    IJwtTokenServices _jwtServices;
    DbManager _db;
    public JWTAuthServices(IJwtTokenServices jwtServices, DbManager db)
    {
        _jwtServices = jwtServices;
        _db = db;
    }
    public async Task<Result<AuthJWTResponse>> IssueTokenAsync(int userId)
    {
        var accessToken = _jwtServices.CreateAccessToken(userId);
        var (raw, hash) = _jwtServices.GenerateRefreshToken();
        await _db.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = userId,
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            }
        );
        await _db.SaveChangesAsync();
        return Result<AuthJWTResponse>.Success(new AuthJWTResponse
            (
                accessToken,
                raw
            )
        );
    }
    public async Task<Result<AuthJWTResponse>> RefreshTokenAsync(string presentedRawToken)
    {
        var presentedHashToken = _jwtServices.HashToken(presentedRawToken);
        var stored = await _db.RefreshTokens
            .Where(t => t.TokenHash == presentedHashToken)
            .FirstOrDefaultAsync();
        if(stored is null)
        {
            return Result<AuthJWTResponse>.Failure("Invalid Refresh Token",StatusCodes.Status401Unauthorized);
        }
        if(stored.RevokedAt is not null)
        {
            var allActive = _db.RefreshTokens.Where(t => t.UserId == stored.UserId);
            await allActive.ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
            return Result<AuthJWTResponse>.Failure("Refresh token reuse detected. All sessions revoked.", StatusCodes.Status401Unauthorized);
        }
        if (!stored.IsActive)
        {
            return Result<AuthJWTResponse>.Failure("Refresh Token Expired", StatusCodes.Status401Unauthorized);
        }
        stored.RevokedAt = DateTime.UtcNow;
        var (newRaw, newHash) = _jwtServices.GenerateRefreshToken();
        stored.ReplacedByTokenHash =newHash;
        await _db.RefreshTokens.AddAsync(_jwtServices.RefreshTokenFactory(stored.UserId, newHash));
        var newAccessToken = _jwtServices.CreateAccessToken(stored.Id);
        await _db.SaveChangesAsync();
        return Result<AuthJWTResponse>.Success(
            new AuthJWTResponse(
                newAccessToken, 
                newRaw
                )
            );
    }
}