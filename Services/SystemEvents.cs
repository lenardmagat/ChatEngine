using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using MediatR;

namespace ChatSystem.SystemEvents;

    public record SendMessageCommand(int UserId, SendMessage MessageData) : IRequest<Result<MessageResponseDTO>>;
    public record InitializeChatCommand(int UserId, string RecieverId) : IRequest<Result<string>>;
    public record IOnConnectAutoJoinChat(int UserId) : IRequest<Result<List<string>>>;
    public record LoadConversationCommand(int UserId) : IRequest<Result<List<LoadConversationResponse>>>;