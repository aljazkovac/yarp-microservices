namespace ProductService.Application;

/// <summary>
/// Data Transfer Object (DTO) representing a Product for external consumers (e.g., API clients).
/// DTOs help decouple the internal domain model from the external contract.
/// Using a record simplifies immutable data carriers.
/// </summary>
public record ProductDto
(
    Guid Id,
    string Name,
    string Description,
    decimal Price
);