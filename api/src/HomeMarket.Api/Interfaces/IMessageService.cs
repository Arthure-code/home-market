using Ardalis.Result;
using HomeMarket.Api.Dtos;

namespace HomeMarket.Api.Interfaces
{
    // Every call is made for the account named by the token: its inbox,
    // its sent folder, a message it sent or received, a message it sends.
    // Sending to nobody is NotFound, to oneself Invalid; marking read is
    // Forbidden to the sender and NotFound to anyone else.
    public interface IMessageService
    {
        Task<IReadOnlyList<MessageDto>> InboxAsync(int readerId);
        Task<IReadOnlyList<MessageDto>> SentAsync(int readerId);
        Task<MessageDetailDto?> GetAsync(int readerId, int id);
        Task<Result<MessageDetailDto>> SendAsync(int senderId, MessageRequest request);
        Task<Result> MarkReadAsync(int readerId, int id);
    }
}
