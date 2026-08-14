using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Routers.Features;
[ApiController]
[Route("API/[Controller]")]
public partial class FeatureController : ControllerBase
{
    private readonly IMediator _mediator;
    public FeatureController(IMediator mediator) => _mediator = mediator;
}