using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Routers.Inventory;
[ApiController]
[Route("/[controller]")]
public partial class InventoryController : ControllerBase
{
    IMediator _mediator;
    ILogger<InventoryController> _logger;
    public InventoryController(IMediator mediator, ILogger<InventoryController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
}