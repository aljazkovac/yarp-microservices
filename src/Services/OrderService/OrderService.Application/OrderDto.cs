namespace OrderService.Application;

/// <summary>
/// DTO representing a customer order.
/// </summary>
public record OrderDto
(
    Guid OrderId,
    Guid CustomerId,
    DateTimeOffset OrderDate,
    string Status, // String representation of the OrderStatus enum
    List<OrderItemDto> Items, // Changed from IReadOnlyList for easier DTO construction/serialization
    decimal TotalPrice
);