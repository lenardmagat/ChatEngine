using ChatSystem.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("API/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountServices _accountServices;
    public AccountController(IAccountServices accountServices) => _accountServices = accountServices;
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccountEndpoint(
        [FromBody] AccountCredentials accountCredentials,
        CancellationToken cancellation)
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