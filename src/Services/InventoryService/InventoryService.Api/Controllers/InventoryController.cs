using InventoryService.Application; // Needed for AppService and DTOs
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers;

/// <summary>
/// API endpoints for managing product inventory.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Base route: /api/inventory
public class InventoryController : ControllerBase
{
    private readonly InventoryAppService _inventoryAppService;

    public InventoryController(InventoryAppService inventoryAppService)
    {
        _inventoryAppService = inventoryAppService ?? throw new ArgumentNullException(nameof(inventoryAppService));
    }

    /// <summary>
    /// Gets inventory status for all products.
    /// GET /api/inventory
    /// </summary>
    /// <returns>A list of inventory items.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InventoryItemDto>), 200)]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAllInventory()
    {
        var items = await _inventoryAppService.GetAllInventoryAsync();
        return Ok(items);
    }

    /// <summary>
    /// Gets inventory status for a specific product.
    /// GET /api/inventory/{productId}
    /// </summary>
    /// <param name="productId">The GUID identifier of the product.</param>
    /// <returns>The inventory status or Not Found.</returns>
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(InventoryItemDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<InventoryItemDto>> GetInventoryByProductId(Guid productId)
    {
        var item = await _inventoryAppService.GetInventoryByProductIdAsync(productId);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }

    // --- Methods to Modify Stock ---
    // Note: Using POST for simplicity. In stricter REST, PUT might set the absolute value,
    // and POST could be used for operations like 'increase' or 'decrease'. PATCH is also an option.

    /// <summary>
    /// Increases the stock for a specific product. Creates the record if it doesn't exist.
    /// POST /api/inventory/{productId}/increase
    /// </summary>
    /// <param name="productId">Product ID.</param>
    /// <param name="request">Request containing the amount to increase.</param>
    /// <returns>The updated inventory status.</returns>
    [HttpPost("{productId:guid}/increase")]
    [ProducesResponseType(typeof(InventoryItemDto), 200)]
    [ProducesResponseType(400)] // Bad request (e.g., negative amount)
    public async Task<ActionResult<InventoryItemDto>> IncreaseStock(Guid productId, [FromBody] UpdateStockRequest request)
    {
        if (request == null || request.Amount <= 0)
        {
            return BadRequest("Amount must be positive.");
        }
        try
        {
            var updatedItem = await _inventoryAppService.IncreaseStockAsync(productId, request.Amount);
            return Ok(updatedItem);
        }
        catch(ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        // Add more robust error handling if needed
    }

    /// <summary>
    /// Decreases the stock for a specific product.
    /// POST /api/inventory/{productId}/decrease
    /// </summary>
    /// <param name="productId">Product ID.</param>
    /// <param name="request">Request containing the amount to decrease.</param>
    /// <returns>The updated inventory status or error if insufficient stock.</returns>
    [HttpPost("{productId:guid}/decrease")]
    [ProducesResponseType(typeof(InventoryItemDto), 200)]
    [ProducesResponseType(400)] // Bad request (e.g., negative amount, insufficient stock)
    [ProducesResponseType(404)] // Not found (if DecreaseStock throws InvalidOperationException for not found)
    public async Task<ActionResult<InventoryItemDto>> DecreaseStock(Guid productId, [FromBody] UpdateStockRequest request)
    {
        if (request == null || request.Amount <= 0)
        {
            return BadRequest("Amount must be positive.");
        }
        try
        {
            var updatedItem = await _inventoryAppService.DecreaseStockAsync(productId, request.Amount);
            return Ok(updatedItem);
        }
        catch (InvalidOperationException ex) // Catch insufficient stock or not found from app service
        {
             // Check message to differentiate? Or have AppService return specific results?
             // For now, map both to BadRequest or potentially NotFound
            if (ex.Message.Contains("not found")) return NotFound(ex.Message);
            return BadRequest(ex.Message);
        }
        catch(ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        // Add more robust error handling if needed
    }

    /// <summary>
    /// Sets the stock quantity directly for a product. Creates the record if it doesn't exist.
    /// PUT /api/inventory/{productId}/stock
    /// </summary>
    /// <param name="productId">Product ID.</param>
    /// <param name="request">Request containing the new quantity.</param>
    /// <returns>The updated inventory status.</returns>
    [HttpPut("{productId:guid}/stock")]
    [ProducesResponseType(typeof(InventoryItemDto), 200)]
    [ProducesResponseType(400)] // Bad request (e.g., negative quantity)
    public async Task<ActionResult<InventoryItemDto>> SetStock(Guid productId, [FromBody] SetStockRequest request)
    {
        if (request == null || request.Quantity < 0)
        {
            return BadRequest("Quantity cannot be negative.");
        }
        try
        {
            var updatedItem = await _inventoryAppService.SetStockAsync(productId, request.Quantity);
            return Ok(updatedItem);
        }
        catch(ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        // Add more robust error handling if needed
    }

}

// --- Request Models for API Layer ---

/// <summary>
/// Request model for increasing or decreasing stock.
/// </summary>
/// <param name="Amount">The amount to change stock by (should be positive).</param>
public record UpdateStockRequest(int Amount);

/// <summary>
/// Request model for setting stock directly.
/// </summary>
/// <param name="Quantity">The desired absolute quantity (should be non-negative).</param>
public record SetStockRequest(int Quantity);
