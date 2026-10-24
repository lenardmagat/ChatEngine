using ChatSystem.ErrorHandling;
using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace ChatSystem.Routers.Account;
public partial class AccountController
{
    [Authorize]
    [EnableRateLimiting("auth")]
    [HttpPatch("AccountUpdate-ChangePassword")]
    public async Task<IActionResult> ChangePasswordEndPoint(
        [FromBody] PasswordCredentials passwordCredentials,
        CancellationToken cancellation
    ){
        int UserId = User.GetUserId()!.Value;
        ChangePasswordCommand command = new ChangePasswordCommand(UserId, passwordCredentials);
        Result result = await _mediator.Send(command, cancellation);
        return result.ToActionResult();
    }
}