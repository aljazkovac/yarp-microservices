using System.ComponentModel.DataAnnotations; // For potential validation attributes later

namespace OrderService.Application;

/// <summary>
/// DTO for creating a new order.
/// </summary>
public record CreateOrderRequestDto
(
    [Required] // Example: CustomerId is required
    Guid CustomerId,

    [Required]
    [MinLength(1)] // Example: Must have at least one item
    List<CreateOrderItemRequestDto> Items
);