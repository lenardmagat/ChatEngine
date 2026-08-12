using ChatSystem.ErrorHandling;
using MediatR;

namespace ChatSystem.SystemEvents.Accounts;

    public record ChangePasswordCommand(int UserId, PasswordCredentials passwordCredentials) : IRequest<Result>;
    public record LoginCommand(AccountCredentials Credentials) : IRequest<Result<LoginResponseData>>;
    public record CreateAccountCommand(AccountCredentials Credentials) : IRequest<Result>;