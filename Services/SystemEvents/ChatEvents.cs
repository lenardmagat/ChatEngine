using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using MediatR;

namespace ChatSystem.SystemEvents.Chats;

    public record SendMessageCommand(int UserId, SendMessage MessageData) : IRequest<Result<MessageResponseDTO>>;
    public record InitializeChatCommand(int UserId, string? RecieverId, string? ChatId) : IRequest<Result<ChatData?>>;
    public record IOnConnectAutoJoinChat(int UserId) : IRequest<Result<string>>;
    public record LoadConversationCommand(int UserId) : IRequest<Result<List<LoadConversationResponse>>>;
    public record GetRoomDataCommand(int UserId, string? ReceiverId, string? RoomId) : IRequest<Result<RoomDataDTO>>;