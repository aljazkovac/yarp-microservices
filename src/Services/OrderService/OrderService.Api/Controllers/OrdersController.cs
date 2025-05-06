using Microsoft.AspNetCore.Mvc;
using OrderService.Application; // For AppService and DTOs

namespace OrderService.Api.Controllers;

/// <summary>
/// API endpoints for managing customer orders.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Base route: /api/orders
public class OrdersController : ControllerBase
{
    private readonly OrderAppService _orderAppService;

    public OrdersController(OrderAppService orderAppService)
    {
        _orderAppService = orderAppService ?? throw new ArgumentNullException(nameof(orderAppService));
    }

    /// <summary>
    /// Creates a new order.
    /// POST /api/orders
    /// </summary>
    /// <param name="request">The order creation request details.</param>
    /// <returns>The created order or an error response.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), 201)] // Created
    [ProducesResponseType(400)] // Bad Request
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        // Add basic validation for the request DTO itself if not using framework validation extensively yet
        if (request == null || request.Items == null || !request.Items.Any())
        {
            return BadRequest("Order request must contain a customer ID and at least one item.");
        }
        // Further validation (e.g., using DataAnnotations on DTOs or FluentValidation) is recommended

        try
        {
            var createdOrder = await _orderAppService.CreateOrderAsync(request);
            // Return HTTP 201 Created with the location of the new resource and the resource itself
            return CreatedAtAction(nameof(GetOrderById), new { orderId = createdOrder.OrderId }, createdOrder);
        }
        catch (ArgumentException ex) // Catch specific exceptions from app/domain layer
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex) // E.g., trying to add to a confirmed order, or product already exists
        {
            return BadRequest(ex.Message); // Or Conflict (409) depending on the exact error
        }
        // Consider more granular error handling for production
    }

    /// <summary>
    /// Gets a specific order by its ID.
    /// GET /api/orders/{orderId}
    /// </summary>
    /// <param name="orderId">The GUID identifier of the order.</param>
    /// <returns>The requested order or Not Found.</returns>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid orderId)
    {
        var order = await _orderAppService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    /// <summary>
    /// Gets all orders for a specific customer.
    /// GET /api/orders/customer/{customerId}
    /// </summary>
    /// <param name="customerId">The GUID identifier of the customer.</param>
    /// <returns>A list of orders for the customer.</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomerId(Guid customerId)
    {
        var orders = await _orderAppService.GetOrdersByCustomerIdAsync(customerId);
        return Ok(orders);
    }

    /// <summary>
    /// Confirms an existing order.
    /// POST /api/orders/{orderId}/confirm
    /// </summary>
    /// <param name="orderId">The ID of the order to confirm.</param>
    /// <returns>The updated order or an error response.</returns>
    [HttpPost("{orderId:guid}/confirm")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(400)] // e.g. order already confirmed, or empty
    [ProducesResponseType(404)] // Order not found
    public async Task<ActionResult<OrderDto>> ConfirmOrder(Guid orderId)
    {
        try
        {
            var confirmedOrder = await _orderAppService.ConfirmOrderAsync(orderId);
            return Ok(confirmedOrder);
        }
        catch (InvalidOperationException ex) // From domain or app service (e.g., order not found, already confirmed)
        {
            // Better to have more specific exceptions or result objects from AppService
            // to differentiate NotFound from other InvalidOperation errors.
            if (ex.Message.Contains("not found")) // Simple check
            {
                return NotFound(ex.Message);
            }
            return BadRequest(ex.Message);
        }
    }
}