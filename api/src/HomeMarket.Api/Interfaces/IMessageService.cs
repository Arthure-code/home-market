using HomeMarket.Api.Dtos;

namespace HomeMarket.Api.Interfaces
{
    public enum MessageOutcome
    {
        Done,
        NotFound,
        NotMine,
        NoSuchRecipient,
        ToSelf,
    }

    // Every call is made for the account named by the token: its inbox,
    // its sent folder, a message it sent or received, a message it sends.
    public interface IMessageService
    {
        Task<IReadOnlyList<MessageDto>> InboxAsync(int readerId);
        Task<IReadOnlyList<MessageDto>> SentAsync(int readerId);
        Task<MessageDetailDto?> GetAsync(int readerId, int id);
        Task<(MessageOutcome Outcome, MessageDetailDto? Message)> SendAsync(int senderId, MessageRequest request);
        Task<MessageOutcome> MarkReadAsync(int readerId, int id);
    }
}
