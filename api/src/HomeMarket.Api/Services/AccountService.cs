using Ardalis.Result;
using HomeMarket.Api.Data;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeMarket.Api.Services
{
    public class AccountService : IAccountService
    {
        private readonly MarketContext _context;
        private readonly IPasswordHasher<Account> _hasher;

        public AccountService(MarketContext context, IPasswordHasher<Account> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task<Result> RegisterAsync(RegisterRequest request)
        {
            var userName = Normalize(request.UserName);
            if (await _context.Accounts.AnyAsync(a => a.UserName == userName)) return Result.Conflict("That user name is taken.");

            var account = new Account { UserName = userName, CreatedAt = DateTime.UtcNow };
            account.PasswordHash = _hasher.HashPassword(account, request.Password);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        // Null for an unknown name and for a wrong password alike; the
        // caller never learns which. The store account has no hash and can
        // therefore never sign in.
        public async Task<Account?> AuthenticateAsync(LoginRequest request)
        {
            var userName = Normalize(request.UserName);
            var account = await _context.Accounts.SingleOrDefaultAsync(a => a.UserName == userName);
            if (account is null || account.PasswordHash.Length == 0) return null;

            var result = _hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
            return result == PasswordVerificationResult.Failed ? null : account;
        }

        private static string Normalize(string userName)
        {
            return userName.Trim().ToLowerInvariant();
        }
    }
}
