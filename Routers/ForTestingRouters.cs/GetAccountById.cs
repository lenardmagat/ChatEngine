using Microsoft.AspNetCore.Mvc;
namespace ChatSystem.Routers.Account;
public partial class AccountController
{
    [HttpGet("get/{AccountId}")]
    public async Task<ActionResult> GetAccountEndpoint(int AccountId, CancellationToken cancellation)
    {
        var result = await _accountServices.GetAccountHashIdService(AccountId, cancellation);
        if(!result.IsSuccess)
            return StatusCode(
                result.StatusCode, new
                {
                    error = result.Error,
                    timestampt = DateTime.UtcNow
                }
            );
        return Ok(result.Value);
    }
}