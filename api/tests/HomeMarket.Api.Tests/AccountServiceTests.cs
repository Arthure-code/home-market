using Ardalis.Result;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Models;
using HomeMarket.Api.Services;
using Microsoft.AspNetCore.Identity;

namespace HomeMarket.Api.Tests
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task RegisterAsync_NewName_StoresItLowercasedWithAHashNotThePassword()
        {
            // Given
            using var market = new Market();
            var service = new AccountService(market.Context, new PasswordHasher<Account>());

            // When
            var result = await service.RegisterAsync(new RegisterRequest { UserName = " Nadia ", Password = "Secret-123" });

            // Then
            Assert.True(result.IsSuccess);
            var account = Assert.Single(market.Context.Accounts);
            Assert.Equal("nadia", account.UserName);
            Assert.NotEmpty(account.PasswordHash);
            Assert.DoesNotContain("Secret-123", account.PasswordHash);
        }

        [Fact]
        public async Task RegisterAsync_NameAlreadyTakenInAnotherCase_IsAConflict()
        {
            // Given
            using var market = new Market();
            market.Member("nadia");
            var service = new AccountService(market.Context, new PasswordHasher<Account>());

            // When
            var result = await service.RegisterAsync(new RegisterRequest { UserName = "NADIA", Password = "Secret-123" });

            // Then
            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Single(market.Context.Accounts);
        }

        [Fact]
        public async Task AuthenticateAsync_RightPassword_ReturnsTheAccount()
        {
            // Given
            using var market = new Market();
            var service = new AccountService(market.Context, new PasswordHasher<Account>());
            await service.RegisterAsync(new RegisterRequest { UserName = "nadia", Password = "Secret-123" });

            // When
            var account = await service.AuthenticateAsync(new LoginRequest { UserName = "Nadia", Password = "Secret-123" });

            // Then
            Assert.Equal("nadia", account!.UserName);
        }

        [Fact]
        public async Task AuthenticateAsync_WrongPasswordOrUnknownName_ReturnsNullAlike()
        {
            // Given
            using var market = new Market();
            var service = new AccountService(market.Context, new PasswordHasher<Account>());
            await service.RegisterAsync(new RegisterRequest { UserName = "nadia", Password = "Secret-123" });

            // When
            var wrongPassword = await service.AuthenticateAsync(new LoginRequest { UserName = "nadia", Password = "Secret-124" });
            var unknownName = await service.AuthenticateAsync(new LoginRequest { UserName = "nobody", Password = "Secret-123" });

            // Then
            Assert.Null(wrongPassword);
            Assert.Null(unknownName);
        }

        [Fact]
        public async Task AuthenticateAsync_StoreAccountWithoutAHash_ReturnsNull()
        {
            // Given
            using var market = new Market();
            market.Member("homemarket", passwordHash: string.Empty);
            var service = new AccountService(market.Context, new PasswordHasher<Account>());

            // When
            var account = await service.AuthenticateAsync(new LoginRequest { UserName = "homemarket", Password = string.Empty });

            // Then
            Assert.Null(account);
        }
    }
}
