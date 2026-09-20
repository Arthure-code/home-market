using HomeMarket.Api.Data;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeMarket.Api.Services
{
    public class MessageService : IMessageService
    {
        private readonly MarketContext _context;

        public MessageService(MarketContext context)
        {
            _context = context;
        }

        public Task<IReadOnlyList<MessageDto>> InboxAsync(int readerId)
        {
            return Project(_context.Messages.Where(m => m.RecipientId == readerId), readerId);
        }

        public Task<IReadOnlyList<MessageDto>> SentAsync(int readerId)
        {
            return Project(_context.Messages.Where(m => m.SenderId == readerId), readerId);
        }

        // A message is found for its sender and its recipient only. For
        // anyone else it does not exist: not 403, which would say it does.
        public async Task<MessageDetailDto?> GetAsync(int readerId, int id)
        {
            return await _context.Messages
                .Where(m => m.Id == id && (m.SenderId == readerId || m.RecipientId == readerId))
                .Select(m => new MessageDetailDto
                {
                    Id = m.Id,
                    From = m.Sender!.UserName,
                    To = m.Recipient!.UserName,
                    Subject = m.Subject,
                    Body = m.Body,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt,
                    Mine = m.SenderId == readerId,
                })
                .SingleOrDefaultAsync();
        }

        // The recipient is looked up by name and must be able to sign in:
        // the store account, which cannot, cannot receive either. Writing
        // to oneself is refused.
        public async Task<(MessageOutcome Outcome, MessageDetailDto? Message)> SendAsync(int senderId, MessageRequest request)
        {
            var to = request.To.Trim().ToLowerInvariant();
            var recipient = await _context.Accounts.SingleOrDefaultAsync(a => a.UserName == to);
            if (recipient is null || recipient.PasswordHash.Length == 0) return (MessageOutcome.NoSuchRecipient, null);
            if (recipient.Id == senderId) return (MessageOutcome.ToSelf, null);

            var message = new Message
            {
                SenderId = senderId,
                RecipientId = recipient.Id,
                Subject = request.Subject.Trim(),
                Body = request.Body.Trim(),
                SentAt = DateTime.UtcNow,
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return (MessageOutcome.Done, await GetAsync(senderId, message.Id));
        }

        // Only the recipient marks a message read; reading it again changes
        // nothing.
        public async Task<MessageOutcome> MarkReadAsync(int readerId, int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message is null || (message.SenderId != readerId && message.RecipientId != readerId)) return MessageOutcome.NotFound;
            if (message.RecipientId != readerId) return MessageOutcome.NotMine;

            if (message.ReadAt is null)
            {
                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return MessageOutcome.Done;
        }

        private static async Task<IReadOnlyList<MessageDto>> Project(IQueryable<Message> messages, int readerId)
        {
            return await messages
                .OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    From = m.Sender!.UserName,
                    To = m.Recipient!.UserName,
                    Subject = m.Subject,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt,
                    Mine = m.SenderId == readerId,
                })
                .ToListAsync();
        }
    }
}
