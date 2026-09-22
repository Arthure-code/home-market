using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Tests
{
    // A refusal is a Problem Details document carrying its status code,
    // whatever the controller and whatever the reason.
    public static class Refusal
    {
        public static ProblemDetails Of(object? result, int statusCode)
        {
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(statusCode, objectResult.StatusCode);
            return Assert.IsAssignableFrom<ProblemDetails>(objectResult.Value);
        }
    }
}
