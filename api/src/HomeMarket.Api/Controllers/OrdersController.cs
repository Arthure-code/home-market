using Ardalis.Result;
using HomeMarket.Api.Auth;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Controllers
{
    // Orders of the account named by the token: placing one from the
    // cart, reading back my own, and the lines other members bought from
    // me.
    [Authorize]
    [ApiController]
    [Route("api/orders")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrdersController(IOrderService orders)
        {
            _orders = orders;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderDto>>> Mine()
        {
            return Ok(await _orders.MineAsync(User.AccountId()));
        }

        [HttpGet("sales")]
        public async Task<ActionResult<IReadOnlyList<SaleDto>>> Sales()
        {
            return Ok(await _orders.SalesAsync(User.AccountId()));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> Get(int id)
        {
            var order = await _orders.GetAsync(User.AccountId(), id);
            return order is null ? NotFound() : Ok(order);
        }

        // A refused card is the service's Error, and here a 402 with the
        // provider's reason; the other refusals translate as everywhere.
        [HttpPost]
        public async Task<ActionResult<OrderDto>> Place(CheckoutRequest request)
        {
            var result = await _orders.PlaceAsync(User.AccountId(), request);
            if (result.IsSuccess) return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
            return result.Status == ResultStatus.Error
                ? Problem(string.Join(" ", result.Errors), statusCode: StatusCodes.Status402PaymentRequired)
                : this.Refuse(result);
        }
    }
}
