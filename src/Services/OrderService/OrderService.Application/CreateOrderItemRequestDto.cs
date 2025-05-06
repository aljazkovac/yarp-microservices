using System.ComponentModel.DataAnnotations; // For potential validation attributes later

namespace OrderService.Application;

/// <summary>
/// DTO for specifying an item to be added to a new order.
/// </summary>
public record CreateOrderItemRequestDto
(
    [Required] // Example: ProductId is required
    Guid ProductId,

    // ProductName and UnitPrice will typically be fetched or validated by the OrderAppService
    // before creating the domain OrderItem, to ensure they are current and correct.
    // For simplicity in this initial DTO, we might not pass them from the client,
    // or pass them for client-side display confirmation but re-verify/fetch on the server.
    // Let's assume for now the client *must* provide them.

    [Required]
    string ProductName,

    [Range(0.01, double.MaxValue)] // Example: UnitPrice must be positive
    decimal UnitPrice,

    [Range(1, int.MaxValue)] // Example: Quantity must be at least 1
    int Quantity
);