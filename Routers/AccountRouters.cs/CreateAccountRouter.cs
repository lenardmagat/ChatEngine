using Microsoft.AspNetCore.Mvc;
namespace ChatSystem.Routers.Account;
public partial class AccountController
{
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccountEndpoint(
            [FromBody] AccountCredentials accountCredentials,
            CancellationToken cancellation
        )
    {
        var result = await _accountServices.CreateAccountService(accountCredentials, cancellation);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new
                {
                    error = result.Error,
                    timestampt = DateTime.UtcNow
                }
            );
        }
        return Ok();
    }
}