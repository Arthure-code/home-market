using AutoFixture;
using HomeMarket.Api.Controllers;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class AccountsControllerTests
    {
        [Fact]
        public async Task Register_NameFree_Returns201()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<RegisterRequest>();
            var accounts = new Mock<IAccountService>();
            var tokens = new Mock<ITokenService>();
            accounts.Setup(a => a.RegisterAsync(request)).ReturnsAsync(RegisterOutcome.Created);
            var controller = new AccountsController(accounts.Object, tokens.Object);

            // When
            var result = await controller.Register(request);

            // Then
            var created = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            accounts.Verify(a => a.RegisterAsync(request), Times.Once);
        }

        [Fact]
        public async Task Register_NameTaken_Returns409()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<RegisterRequest>();
            var accounts = new Mock<IAccountService>();
            var tokens = new Mock<ITokenService>();
            accounts.Setup(a => a.RegisterAsync(request)).ReturnsAsync(RegisterOutcome.NameTaken);
            var controller = new AccountsController(accounts.Object, tokens.Object);

            // When
            var result = await controller.Register(request);

            // Then
            Refusal.Of(result, StatusCodes.Status409Conflict);
        }

        [Fact]
        public async Task Login_WrongNameOrPassword_Returns401WithoutAToken()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<LoginRequest>();
            var accounts = new Mock<IAccountService>();
            var tokens = new Mock<ITokenService>();
            accounts.Setup(a => a.AuthenticateAsync(request)).ReturnsAsync((Account?)null);
            var controller = new AccountsController(accounts.Object, tokens.Object);

            // When
            var result = await controller.Login(request);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status401Unauthorized);
            tokens.Verify(t => t.Issue(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public async Task Login_KnownAccount_ReturnsTheSessionWithItsToken()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<LoginRequest>();
            var account = fixture.Build<Account>().Without(a => a.Listings).Without(a => a.Liked).Create();
            var expiresAt = DateTime.UtcNow.AddHours(1);
            var accounts = new Mock<IAccountService>();
            var tokens = new Mock<ITokenService>();
            accounts.Setup(a => a.AuthenticateAsync(request)).ReturnsAsync(account);
            tokens.Setup(t => t.Issue(account)).Returns(("signed-token", expiresAt));
            var controller = new AccountsController(accounts.Object, tokens.Object);

            // When
            var result = await controller.Login(request);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var session = Assert.IsType<SessionDto>(ok.Value);
            Assert.Equal(account.UserName, session.UserName);
            Assert.Equal("signed-token", session.Token);
            Assert.Equal(expiresAt, session.ExpiresAt);
        }
    }
}
