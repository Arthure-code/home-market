using HomeMarket.Api.Auth;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Controllers
{
    // Messages between accounts. Everything here is for the account named
    // by the token: no user name in the address, no sender in the body.
    [Authorize]
    [ApiController]
    [Route("api/messages")]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messages;

        public MessagesController(IMessageService messages)
        {
            _messages = messages;
        }

        [HttpGet("inbox")]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> Inbox()
        {
            return Ok(await _messages.InboxAsync(User.AccountId()));
        }

        [HttpGet("sent")]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> Sent()
        {
            return Ok(await _messages.SentAsync(User.AccountId()));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MessageDetailDto>> Get(int id)
        {
            var message = await _messages.GetAsync(User.AccountId(), id);
            return message is null ? NotFound() : Ok(message);
        }

        [HttpPost]
        public async Task<ActionResult<MessageDetailDto>> Send(MessageRequest request)
        {
            var (outcome, message) = await _messages.SendAsync(User.AccountId(), request);
            return outcome switch
            {
                MessageOutcome.NoSuchRecipient => NotFound(new { message = "No member has that user name." }),
                MessageOutcome.ToSelf => BadRequest(new { message = "You cannot message yourself." }),
                _ => CreatedAtAction(nameof(Get), new { id = message!.Id }, message),
            };
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            return await _messages.MarkReadAsync(User.AccountId(), id) switch
            {
                MessageOutcome.NotFound => NotFound(),
                MessageOutcome.NotMine => Forbid(),
                _ => NoContent(),
            };
        }
    }
}
