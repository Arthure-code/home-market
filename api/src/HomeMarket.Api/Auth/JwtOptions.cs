namespace HomeMarket.Api.Auth
{
    // Read from the "Jwt" section. The key never lives in the repository:
    // user secrets or an environment variable in development, a vault in
    // production. Sessions last an hour.
    public class JwtOptions
    {
        public const string Section = "Jwt";

        public string Issuer { get; set; } = "home-market";
        public string Audience { get; set; } = "home-market";
        public string Key { get; set; } = string.Empty;
        public int LifetimeMinutes { get; set; } = 60;
    }
}
