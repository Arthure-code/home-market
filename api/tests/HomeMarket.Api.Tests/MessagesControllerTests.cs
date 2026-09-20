using AutoFixture;
using HomeMarket.Api.Controllers;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class MessagesControllerTests
    {
        private const int Omar = 12;

        [Fact]
        public async Task Inbox_ReturnsWhatWasSentToTheCaller()
        {
            // Given
            var fixture = new Fixture();
            var inbox = fixture.CreateMany<MessageDto>(2).ToList();
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.InboxAsync(Omar)).ReturnsAsync(inbox);
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.Inbox();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(inbox, ok.Value);
        }

        [Fact]
        public async Task Sent_ReturnsWhatTheCallerSent()
        {
            // Given
            var fixture = new Fixture();
            var sent = fixture.CreateMany<MessageDto>(3).ToList();
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.SentAsync(Omar)).ReturnsAsync(sent);
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.Sent();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(sent, ok.Value);
        }

        [Fact]
        public async Task Get_MessageOfTheCaller_ReturnsItWithItsBody()
        {
            // Given
            var fixture = new Fixture();
            var message = fixture.Create<MessageDetailDto>();
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.GetAsync(Omar, message.Id)).ReturnsAsync(message);
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.Get(message.Id);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(message, ok.Value);
        }

        [Fact]
        public async Task Get_MessageOfSomebodyElse_Returns404()
        {
            // Given
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.GetAsync(Omar, 41)).ReturnsAsync((MessageDetailDto?)null);
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.Get(41);

            // Then
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Send_ToAMember_Returns201PointingAtTheMessage()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<MessageRequest>();
            var message = fixture.Create<MessageDetailDto>();
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.SendAsync(Omar, request)).ReturnsAsync((MessageOutcome.Done, message));
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.Send(request);

            // Then
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(MessagesController.Get), created.ActionName);
            Assert.Equal(message.Id, created.RouteValues!["id"]);
            Assert.Same(message, created.Value);
        }

        [Fact]
        public async Task Send_ToNobodyOrToTheStore_Returns404()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<MessageRequest>();
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.SendAsync(Omar, request)).ReturnsAsync((MessageOutcome.NoSuchRecipient, null));
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.Send(request);

            // Then
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Send_ToOneself_Returns400()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<MessageRequest>();
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.SendAsync(Omar, request)).ReturnsAsync((MessageOutcome.ToSelf, null));
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.Send(request);

            // Then
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task MarkRead_ByTheRecipient_Returns204()
        {
            // Given
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.MarkReadAsync(Omar, 41)).ReturnsAsync(MessageOutcome.Done);
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.MarkRead(41);

            // Then
            Assert.IsType<NoContentResult>(result);
            messages.Verify(m => m.MarkReadAsync(Omar, 41), Times.Once);
        }

        [Fact]
        public async Task MarkRead_ByTheSender_Returns403()
        {
            // Given
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.MarkReadAsync(Omar, 41)).ReturnsAsync(MessageOutcome.NotMine);
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.MarkRead(41);

            // Then
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task MarkRead_MessageOfSomebodyElse_Returns404()
        {
            // Given
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.MarkReadAsync(Omar, 41)).ReturnsAsync(MessageOutcome.NotFound);
            var controller = new MessagesController(messages.Object) { ControllerContext = Caller.Member(Omar) };

            // When
            var result = await controller.MarkRead(41);

            // Then
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
