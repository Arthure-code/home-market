using Ardalis.Result;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Services;

namespace HomeMarket.Api.Tests
{
    public class MessageServiceTests
    {
        [Fact]
        public async Task InboxAsync_ReturnsWhatWasReceivedNewestFirstNotWhatWasSent()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var older = market.Sent(omar, nadia, "First");
            var newer = market.Sent(omar, nadia, "Second");
            newer.SentAt = older.SentAt.AddMinutes(1);
            market.Sent(nadia, omar, "Reply");
            market.Context.SaveChanges();
            var service = new MessageService(market.Context);

            // When
            var inbox = await service.InboxAsync(nadia.Id);

            // Then
            Assert.Equal(new[] { "Second", "First" }, inbox.Select(m => m.Subject));
            Assert.All(inbox, m => Assert.False(m.Mine));
            Assert.Equal("omar", inbox[0].From);
        }

        [Fact]
        public async Task SentAsync_ReturnsWhatWasSentMarkedMine()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            market.Sent(omar, nadia, "Hello");
            market.Sent(nadia, omar, "Reply");
            var service = new MessageService(market.Context);

            // When
            var sent = await service.SentAsync(nadia.Id);

            // Then
            var message = Assert.Single(sent);
            Assert.Equal("Reply", message.Subject);
            Assert.True(message.Mine);
            Assert.Equal("omar", message.To);
        }

        [Fact]
        public async Task GetAsync_ByAThirdParty_ReturnsNull()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var eve = market.Member("eve");
            var message = market.Sent(omar, nadia, "Private");
            var service = new MessageService(market.Context);

            // When
            var asRecipient = await service.GetAsync(nadia.Id, message.Id);
            var asSender = await service.GetAsync(omar.Id, message.Id);
            var asStranger = await service.GetAsync(eve.Id, message.Id);

            // Then
            Assert.Equal("Private body", asRecipient!.Body);
            Assert.False(asRecipient.Mine);
            Assert.True(asSender!.Mine);
            Assert.Null(asStranger);
        }

        [Fact]
        public async Task SendAsync_ToAMember_StoresItTrimmedAndReturnsIt()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            market.Member("omar");
            var service = new MessageService(market.Context);

            // When
            var result = await service.SendAsync(nadia.Id, new MessageRequest { To = " Omar ", Subject = " Hello ", Body = " Is the lamp still for sale? " });

            // Then
            Assert.True(result.IsSuccess);
            Assert.Equal("nadia", result.Value.From);
            Assert.Equal("omar", result.Value.To);
            Assert.Equal("Hello", result.Value.Subject);
            Assert.Equal("Is the lamp still for sale?", result.Value.Body);
            Assert.True(result.Value.Mine);
            Assert.Null(result.Value.ReadAt);
        }

        [Fact]
        public async Task SendAsync_ToAnUnknownName_IsNotFound()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var service = new MessageService(market.Context);

            // When
            var result = await service.SendAsync(nadia.Id, new MessageRequest { To = "nobody", Subject = "Hello", Body = "..." });

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Empty(market.Context.Messages);
        }

        [Fact]
        public async Task SendAsync_ToTheStoreAccount_IsNotFound()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            market.Member("homemarket", passwordHash: string.Empty);
            var service = new MessageService(market.Context);

            // When
            var result = await service.SendAsync(nadia.Id, new MessageRequest { To = "homemarket", Subject = "Hello", Body = "..." });

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task SendAsync_ToOneself_IsInvalid()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var service = new MessageService(market.Context);

            // When
            var result = await service.SendAsync(nadia.Id, new MessageRequest { To = "nadia", Subject = "Hello", Body = "..." });

            // Then
            Assert.Equal(ResultStatus.Invalid, result.Status);
            Assert.Equal("To", Assert.Single(result.ValidationErrors).Identifier);
        }

        [Fact]
        public async Task MarkReadAsync_ByTheRecipient_SetsReadAtOnceAndKeepsIt()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var message = market.Sent(omar, nadia, "Hello");
            var service = new MessageService(market.Context);

            // When
            var first = await service.MarkReadAsync(nadia.Id, message.Id);
            var readAt = market.Context.Messages.Single().ReadAt;
            var second = await service.MarkReadAsync(nadia.Id, message.Id);

            // Then
            Assert.True(first.IsSuccess);
            Assert.NotNull(readAt);
            Assert.True(second.IsSuccess);
            Assert.Equal(readAt, market.Context.Messages.Single().ReadAt);
        }

        [Fact]
        public async Task MarkReadAsync_ByTheSender_IsForbidden()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var message = market.Sent(omar, nadia, "Hello");
            var service = new MessageService(market.Context);

            // When
            var result = await service.MarkReadAsync(omar.Id, message.Id);

            // Then
            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Null(market.Context.Messages.Single().ReadAt);
        }

        [Fact]
        public async Task MarkReadAsync_ByAStrangerOrForAMissingMessage_IsNotFound()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var eve = market.Member("eve");
            var message = market.Sent(omar, nadia, "Hello");
            var service = new MessageService(market.Context);

            // When
            var asStranger = await service.MarkReadAsync(eve.Id, message.Id);
            var missing = await service.MarkReadAsync(nadia.Id, 999);

            // Then
            Assert.Equal(ResultStatus.NotFound, asStranger.Status);
            Assert.Equal(ResultStatus.NotFound, missing.Status);
        }
    }
}
