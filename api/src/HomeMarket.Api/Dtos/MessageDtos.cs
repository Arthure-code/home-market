using System.ComponentModel.DataAnnotations;

namespace HomeMarket.Api.Dtos
{
    // What a member sends: the recipient by user name, a subject and the
    // text. The sender is never in the request; it is the token.
    public class MessageRequest
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string To { get; set; } = string.Empty;

        [Required]
        [StringLength(120, MinimumLength = 1)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Body { get; set; } = string.Empty;
    }

    // A line of the inbox or of the sent folder: no body, so the list stays
    // light. Mine is true for a message I sent.
    public class MessageDto
    {
        public int Id { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool Mine { get; set; }
    }

    public class MessageDetailDto : MessageDto
    {
        public string Body { get; set; } = string.Empty;
    }
}
