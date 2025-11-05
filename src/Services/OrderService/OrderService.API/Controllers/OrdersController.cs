using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.Queries;

namespace OrderService.API.Controllers;

/// <summary>
/// کنترلر سفارش‌ها
/// این کنترلر از CQRS Pattern با MediatR استفاده می‌کند
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// دریافت تمام سفارش‌ها
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllOrders()
    {
        _logger.LogInformation("Getting all orders");
        var query = new GetAllOrdersQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return StatusCode(500, new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    /// <summary>
    /// دریافت سفارش بر اساس ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        _logger.LogInformation("Getting order with id: {OrderId}", id);
        var query = new GetOrderByIdQuery { OrderId = id };
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            if (result.ErrorMessage.Contains("not found"))
                return NotFound(new { error = result.ErrorMessage });
            
            return StatusCode(500, new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// حذف سفارش
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        _logger.LogInformation("Deleting order with id: {OrderId}", id);
        var deleted = await _mediator.Send(new OrderService.Application.Commands.DeleteOrderCommand { OrderId = id });
        if (deleted.IsFailure)
        {
            if (deleted.ErrorMessage.Contains("not found"))
                return NotFound(new { error = deleted.ErrorMessage });
            return BadRequest(new { error = deleted.ErrorMessage });
        }
        return Ok(new { success = true });
    }

    /// <summary>
    /// ایجاد سفارش جدید
    /// Idempotency Key می‌تواند از Header یا Body ارسال شود
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        _logger.LogInformation("Creating order for customer: {CustomerId}", command.CustomerId);
        
        // خواندن Idempotency Key از Header (اگر در Body نباشد)
        if (string.IsNullOrEmpty(command.IdempotencyKey))
            command.IdempotencyKey = Request.Headers[Shared.Common.HeaderNames.IdempotencyKey].FirstOrDefault();
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.ErrorMessage });

        return CreatedAtAction(nameof(GetOrderById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// تایید پرداخت سفارش و نهایی‌سازی موجودی رزرو شده
    /// </summary>
    [HttpPost("{id}/confirm-payment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentCommand command)
    {
        _logger.LogInformation("Confirming payment for order: {OrderId}", id);

        if (string.IsNullOrEmpty(command.IdempotencyKey))
            command.IdempotencyKey = Request.Headers[Shared.Common.HeaderNames.IdempotencyKey].FirstOrDefault();

        command.OrderId = id;
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.ErrorMessage.Contains("not found"))
                return NotFound(new { error = result.ErrorMessage });

            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Payment confirmed and inventory committed" });
    }

    /// <summary>
    /// تغییر وضعیت سفارش
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
    {
        _logger.LogInformation("Updating order status: {OrderId} to {Status}", id, command.NewStatus);
        
        command.OrderId = id;
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.ErrorMessage.Contains("not found"))
                return NotFound(new { error = result.ErrorMessage });
            
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Order status updated successfully" });
    }
}

