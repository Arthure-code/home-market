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
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IAccountService> _accounts = new Mock<IAccountService>();
        private readonly Mock<ITokenService> _tokens = new Mock<ITokenService>();
        private readonly AccountsController _controller;

        public AccountsControllerTests()
        {
            _controller = new AccountsController(_accounts.Object, _tokens.Object);
        }

        [Fact]
        public async Task Register_NameFree_Returns201()
        {
            // Given
            var request = _fixture.Create<RegisterRequest>();
            _accounts.Setup(a => a.RegisterAsync(request)).ReturnsAsync(RegisterOutcome.Created);

            // When
            var result = await _controller.Register(request);

            // Then
            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            _accounts.Verify(a => a.RegisterAsync(request), Times.Once);
        }

        [Fact]
        public async Task Register_NameTaken_Returns409()
        {
            // Given
            var request = _fixture.Create<RegisterRequest>();
            _accounts.Setup(a => a.RegisterAsync(request)).ReturnsAsync(RegisterOutcome.NameTaken);

            // When
            var result = await _controller.Register(request);

            // Then
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Login_WrongNameOrPassword_Returns401WithoutAToken()
        {
            // Given
            var request = _fixture.Create<LoginRequest>();
            _accounts.Setup(a => a.AuthenticateAsync(request)).ReturnsAsync((Account?)null);

            // When
            var result = await _controller.Login(request);

            // Then
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            _tokens.Verify(t => t.Issue(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public async Task Login_KnownAccount_ReturnsTheSessionWithItsToken()
        {
            // Given
            var request = _fixture.Create<LoginRequest>();
            var account = _fixture.Build<Account>().Without(a => a.Listings).Without(a => a.Liked).Create();
            var expiresAt = DateTime.UtcNow.AddHours(1);
            _accounts.Setup(a => a.AuthenticateAsync(request)).ReturnsAsync(account);
            _tokens.Setup(t => t.Issue(account)).Returns(("signed-token", expiresAt));

            // When
            var result = await _controller.Login(request);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var session = Assert.IsType<SessionDto>(ok.Value);
            Assert.Equal(account.UserName, session.UserName);
            Assert.Equal("signed-token", session.Token);
            Assert.Equal(expiresAt, session.ExpiresAt);
        }
    }
}
