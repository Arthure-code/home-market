using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Tests
{
    // Who is calling the controller under test: a member whose token
    // names an account id, or a visitor with no token at all.
    public static class Caller
    {
        public static ControllerContext Member(int accountId)
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, accountId.ToString()) }, "Test");
            return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
        }

        public static ControllerContext Visitor()
        {
            return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) } };
        }
    }
}
