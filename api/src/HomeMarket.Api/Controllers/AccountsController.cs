using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HomeMarket.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accounts;
        private readonly ITokenService _tokens;

        public AccountsController(IAccountService accounts, ITokenService tokens)
        {
            _accounts = accounts;
            _tokens = tokens;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _accounts.RegisterAsync(request);
            return result.IsSuccess ? StatusCode(StatusCodes.Status201Created) : this.Refuse(result);
        }

        // One answer for a wrong name and a wrong password, and a limit on
        // attempts, so nobody can list accounts or guess at leisure.
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<ActionResult<SessionDto>> Login(LoginRequest request)
        {
            var account = await _accounts.AuthenticateAsync(request);
            if (account is null) return Problem("Wrong user name or password.", statusCode: StatusCodes.Status401Unauthorized);

            var (token, expiresAt) = _tokens.Issue(account);
            return Ok(new SessionDto { UserName = account.UserName, Token = token, ExpiresAt = expiresAt });
        }
    }
}
