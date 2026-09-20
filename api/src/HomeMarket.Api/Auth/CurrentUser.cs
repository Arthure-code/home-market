using System.Security.Claims;

namespace HomeMarket.Api.Auth
{
    public static class CurrentUser
    {
        // The account id the token was issued for. Only reachable behind
        // [Authorize], so the claim is always there.
        public static int AccountId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // The same, or null for a visitor on a public route.
        public static int? AccountIdOrNull(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return value is null ? null : int.Parse(value);
        }
    }
}
