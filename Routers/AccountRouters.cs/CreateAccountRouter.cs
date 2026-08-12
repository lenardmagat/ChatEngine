using ChatSystem.ErrorHandling.Extension;
using ChatSystem.SystemEvents.Accounts;
using Microsoft.AspNetCore.Mvc;
namespace ChatSystem.Routers.Account;
public partial class AccountController
{
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccountEndpoint(
            [FromBody] AccountCredentials accountCredentials
        )
    {
        CreateAccountCommand command = new CreateAccountCommand(accountCredentials);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}