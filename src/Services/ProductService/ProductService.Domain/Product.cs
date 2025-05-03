// Defines the namespace for the domain layer of the ProductService.
// Following DDD, this layer contains the core business logic and domain objects.
namespace ProductService.Domain;

/// <summary>
/// Represents the Product entity.
/// In DDD terms, Product is also the Aggregate Root for the Product Aggregate.
/// This means it's the main entry point for accessing and modifying product data
/// and is responsible for maintaining the consistency (invariants) of the aggregate.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets the unique identifier for the Product entity.
    /// The Id is the defining characteristic of an Entity in DDD.
    /// </summary>
    public Guid Id { get; private set; } // Private set enforces encapsulation; Id is typically set once on creation.

    /// <summary>
    /// Gets the name of the product.
    /// </summary>
    public string Name { get; private set; } = string.Empty; // Private set encourages changing state via methods on the AR.

    /// <summary>
    /// Gets the description of the product.
    /// </summary>
    public string Description { get; private set; } = string.Empty; // Private set encourages changing state via methods on the AR.

    /// <summary>
    /// Gets the price of the product.
    /// Could potentially be refactored into a Value Object (e.g., Money) later if price logic becomes complex.
    /// </summary>
    public decimal Price { get; private set; } // Private set encourages changing state via methods on the AR.

    /// <summary>
    /// Private constructor for ORM/serialization frameworks.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Product() { }
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the Product aggregate root.
    /// This constructor acts as a factory for creating valid Product instances.
    /// It ensures that a product always starts with essential information.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The product name.</param>
    /// <param name="description">The product description.</param>
    /// <param name="price">The product price.</param>
    public Product(Guid id, string name, string description, decimal price)
    {
        // Basic validation to enforce invariants upon creation
        if (id == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be empty.", nameof(description));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }

    // Future methods for modifying state would go here, e.g.:
    // public void UpdatePrice(decimal newPrice) { ... enforce invariants ... Price = newPrice; }
    // public void Rename(string newName) { ... enforce invariants ... Name = newName; }
}
