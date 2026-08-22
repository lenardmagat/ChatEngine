using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using MediatR;

namespace ChatSystem.SystemEvents.Inventory;
public record CreateProductCommand(int UserId, ProductDetails Details) : IRequest<Result>;