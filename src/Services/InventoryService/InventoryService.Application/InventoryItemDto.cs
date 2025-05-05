namespace InventoryService.Application;

/// <summary>
/// DTO representing the inventory status of a product.
/// </summary>
/// <param name="ProductId">The unique product identifier.</param>
/// <param name="QuantityOnHand">The current quantity in stock.</param>
/// <param name="LastUpdated">Timestamp of the last update.</param>
public record InventoryItemDto
(
    Guid ProductId,
    int QuantityOnHand,
    DateTimeOffset LastUpdated
);