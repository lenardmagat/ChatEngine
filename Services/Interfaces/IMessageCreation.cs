using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;

namespace ChatSystem.Services.Interfaces;
public interface IMessageStrategy
{
    public MessageType Target {get;}
    Task<Result<MessageResponseDTO>> MessageHandler(int UserId, SendMessage request, CancellationToken cancellationToken = default);
}