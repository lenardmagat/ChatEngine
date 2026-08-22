using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using MediatR;

namespace ChatSystem.SystemEvents.Inventory;
public record CreateItemCommand(int UserId, ProductDetails Details) : IRequest<Result>;