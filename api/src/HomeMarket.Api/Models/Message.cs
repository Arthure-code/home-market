namespace HomeMarket.Api.Models
{
    // A note from one account to another. ReadAt stays null until the
    // recipient opens it.
    public class Message
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public int SenderId { get; set; }
        public Account? Sender { get; set; }
        public int RecipientId { get; set; }
        public Account? Recipient { get; set; }
    }
}
