using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using MediatR;
namespace ChatSystem.Hubs;
[Authorize]
public partial class AppHub : Hub
{
    private readonly IMediator _mediator;
    public AppHub(IMediator mediator) => _mediator = mediator;
}