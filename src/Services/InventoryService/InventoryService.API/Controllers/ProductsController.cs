using MediatR;
using Microsoft.AspNetCore.Mvc;
using InventoryService.Application.Commands;
using InventoryService.Application.Queries;

namespace InventoryService.API.Controllers;

/// <summary>
/// کنترلر محصولات
/// این کنترلر از CQRS Pattern با MediatR استفاده می‌کند
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// دریافت تمام محصولات
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProducts()
    {
        _logger.LogInformation("Getting all products");
        var query = new GetAllProductsQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return StatusCode(500, new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    /// <summary>
    /// دریافت محصول بر اساس ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        _logger.LogInformation("Getting product with id: {ProductId}", id);
        var query = new GetProductByIdQuery { ProductId = id };
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
    /// ایجاد محصول جدید
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        _logger.LogInformation("Creating product: {ProductName}", command.Name);
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.ErrorMessage });

        return CreatedAtAction(nameof(GetProductById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// ویرایش محصول
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        _logger.LogInformation("Updating product: {ProductId}", id);
        command.ProductId = id;
        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            if (result.ErrorMessage.Contains("not found"))
                return NotFound(new { error = result.ErrorMessage });
            return BadRequest(new { error = result.ErrorMessage });
        }
        return Ok(new { success = true });
    }

    /// <summary>
    /// حذف محصول
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        _logger.LogInformation("Deleting product: {ProductId}", id);
        var result = await _mediator.Send(new DeleteProductCommand { ProductId = id });
        if (result.IsFailure)
        {
            if (result.ErrorMessage.Contains("not found"))
                return NotFound(new { error = result.ErrorMessage });
            return BadRequest(new { error = result.ErrorMessage });
        }
        return Ok(new { success = true });
    }

    /// <summary>
    /// رزرو موجودی محصول
    /// این متد از Distributed Lock برای جلوگیری از Race Condition استفاده می‌کند
    /// Idempotency Key می‌تواند از Header یا Body ارسال شود
    /// </summary>
    [HttpPost("{id}/reserve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReserveProduct(Guid id, [FromBody] ReserveProductCommand command)
    {
        _logger.LogInformation("Reserving product: {ProductId}, Quantity: {Quantity}", id, command.Quantity);
        
        // خواندن Idempotency Key از Header (اگر در Body نباشد)
           if (string.IsNullOrEmpty(command.IdempotencyKey))
               command.IdempotencyKey = Request.Headers[Shared.Common.HeaderNames.IdempotencyKey].FirstOrDefault();
        
        command.ProductId = id;
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.ErrorMessage.Contains("not found"))
                return NotFound(new { error = result.ErrorMessage });
            
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Product reserved successfully" });
    }

    /// <summary>
    /// آزادسازی موجودی رزرو شده محصول
    /// </summary>
    [HttpPost("{id}/release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReleaseProduct(Guid id, [FromBody] ReleaseProductCommand command)
    {
        _logger.LogInformation("Releasing product: {ProductId}, Quantity: {Quantity}", id, command.Quantity);
        
        command.ProductId = id;
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.ErrorMessage.Contains("not found"))
                return NotFound(new { error = result.ErrorMessage });
            
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Product released successfully" });
    }

    /// <summary>
    /// تایید نهایی سفارش و کاهش موجودی واقعی از رزروشده
    /// </summary>
    [HttpPost("{id}/commit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CommitProduct(Guid id, [FromBody] ConfirmProductCommand command)
    {
        _logger.LogInformation("Committing product: {ProductId}, Quantity: {Quantity}", id, command.Quantity);
        
        // خواندن Idempotency Key از Header (اگر در Body نباشد)
        if (string.IsNullOrEmpty(command.IdempotencyKey))
            command.IdempotencyKey = Request.Headers[Shared.Common.HeaderNames.IdempotencyKey].FirstOrDefault();

        command.ProductId = id;
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.ErrorMessage.Contains("not found"))
                return NotFound(new { error = result.ErrorMessage });
            
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Product committed successfully" });
    }
}

