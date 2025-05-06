namespace OrderService.Application;

/// <summary>
/// DTO representing an item within an order.
/// </summary>
public record OrderItemDto
(
    Guid OrderItemId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);