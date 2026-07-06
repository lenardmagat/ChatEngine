using ChatSystem.ErrorHandling;
using ChatSystem.Services;
using Microsoft.AspNetCore.Mvc;
namespace ChatSystem.Routers.Account;
[ApiController]
[Route("API/[controller]")]
public partial class AccountController : ControllerBase
{
    private readonly IAccountServices _accountServices;
    public AccountController(IAccountServices accountServices) => _accountServices = accountServices;
}