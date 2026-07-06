using Microsoft.AspNetCore.Mvc;
namespace ChatSystem.Routers.Account;
public partial class AccountController
{
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAccountEndpoint(
        [FromBody] AccountCredentials AccountData,
        CancellationToken cancellation)
    {
        var result = await _accountServices.LoginAccountService(AccountData, cancellation);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new
                {
                    error = result.Error,
                    timestampt = DateTime.UtcNow
                }
            );
        }
        return Ok(result.Value);
    }
}