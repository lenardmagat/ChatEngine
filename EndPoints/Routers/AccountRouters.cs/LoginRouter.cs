using ChatSystem.ErrorHandling.Extension;
using ChatSystem.SystemEvents.Accounts;
using Microsoft.AspNetCore.Mvc;
namespace ChatSystem.Routers.Account;
public partial class AccountController
{
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAccountEndpoint(
        [FromBody] AccountCredentials AccountData,
        CancellationToken cancellation
        )
    {
        LoginCommand command = new LoginCommand(AccountData);
        var result = await _mediator.Send(command, cancellation);
        return result.ToActionResult();
    }
}