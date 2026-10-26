using ChatSystem.ErrorHandling;
using ChatSystem.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace ChatSystem.Routers.Account;
[ApiController]
[Route("/[controller]")]
public partial class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }
}