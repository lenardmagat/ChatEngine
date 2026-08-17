namespace ChatSystem.DTOs;
public record AuthJWTResponse(
    string AccessToken,
    string RawToken
);