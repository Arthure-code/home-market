using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using HomeMarket.Api.Auth;
using HomeMarket.Api.Models;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Services;
using HomeMarket.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

namespace HomeMarket.Api
{
    public class Program
    {
        // Never instantiated: everything here is static.
        protected Program() { }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRouting(options => options.LowercaseUrls = true);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<MarketContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("Market") ?? "Data Source=market.db"));

            builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
            builder.Services.AddSingleton<IPhotoStore, PhotoStore>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddSingleton<ITokenService, JwtTokenService>();

            // The signing key comes from configuration (user secrets or the
            // environment). While developing, a missing key is generated for
            // the run: tokens then die with the process, which is fine there.
            var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
            if (string.IsNullOrWhiteSpace(jwt.Key))
            {
                if (!builder.Environment.IsDevelopment())
                {
                    throw new InvalidOperationException("Jwt:Key is not configured.");
                }
                jwt.Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            }
            builder.Services.Configure<JwtOptions>(options =>
            {
                options.Issuer = jwt.Issuer;
                options.Audience = jwt.Audience;
                options.Key = jwt.Key;
                options.LifetimeMinutes = jwt.LifetimeMinutes;
            });

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                    };
                });
            builder.Services.AddAuthorization();

            // Login attempts are counted per client address.
            var loginAttemptsPerMinute = builder.Configuration.GetValue("RateLimiting:LoginAttemptsPerMinute", 5);
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = loginAttemptsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                    }));
            });

            // Only the front end may call from a browser.
            var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MarketContext>();
                context.Database.EnsureCreated();
                Seed(context);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            var images = Path.Combine(app.Environment.ContentRootPath, "images");
            Directory.CreateDirectory(images);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(images),
                RequestPath = "/images",
            });
            app.Use(async (context, next) =>
            {
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers.XFrameOptions = "DENY";
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                if (!context.Request.Path.StartsWithSegments("/images"))
                {
                    context.Response.Headers.CacheControl = "no-store";
                }
                await next();
            });
            app.UseCors();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        // The store account owns the opening catalogue. It has no password
        // hash, so nobody can sign in as it.
        private static void Seed(MarketContext context)
        {
            if (context.Accounts.Any()) return;

            var store = new Account { UserName = Catalogue.StoreAccount, CreatedAt = DateTime.UtcNow };
            context.Accounts.Add(store);
            context.SaveChanges();
            context.Products.AddRange(Catalogue.Listings(store.Id, DateTime.UtcNow.AddDays(-30)));
            context.SaveChanges();
        }
    }
}
