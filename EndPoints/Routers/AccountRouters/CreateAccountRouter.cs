using ChatSystem.ErrorHandling.Extension;
using ChatSystem.SystemEvents.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace ChatSystem.Routers.Account;
public partial class AccountController
{
    [HttpPost("Create")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CreateAccountEndpoint(
            [FromForm] AccountCredentials accountCredentials,
            CancellationToken cancellation
        )
    {
        CreateAccountCommand command = new CreateAccountCommand(accountCredentials);
        var result = await _mediator.Send(command, cancellation);
        return result.ToActionResult();
    }
}