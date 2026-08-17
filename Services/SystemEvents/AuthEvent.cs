using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using MediatR;
namespace ChatSystem.SystemEvents.Auth;
public record LoginCommand(AccountCredentials Credentials) : IRequest<Result<AuthJWTResponse>>;
