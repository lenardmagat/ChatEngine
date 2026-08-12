using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.Extensions.Logging;
namespace ChatSystem.Hubs;
[Authorize]
public partial class AppHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppHub> _logger;
    public AppHub(IMediator mediator, ILogger<AppHub> logger) { 
        _mediator = mediator;
        _logger = logger;
        }
    
}