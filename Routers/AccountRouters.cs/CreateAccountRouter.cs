using ChatSystem.ErrorHandling.Extension;
using ChatSystem.SystemEvents.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ChatSystem.Routers.Account;
public partial class AccountController
{
    [Authorize]
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccountEndpoint(
            [FromBody] AccountCredentials accountCredentials,
            CancellationToken cancellation
        )
    {
        CreateAccountCommand command = new CreateAccountCommand(accountCredentials);
        var result = await _mediator.Send(command, cancellation);
        return result.ToActionResult();
    }
}