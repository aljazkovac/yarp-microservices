namespace ProductService.Domain;

/// <summary>
/// Defines the contract for repository operations related to the Product aggregate root.
/// In DDD, the repository interface belongs to the Domain layer,
/// while its implementation resides in the Infrastructure layer.
/// This decouples the domain logic from specific data access technologies.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves a Product entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the product entity or null if not found.</returns>
    Task<Product?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all Product entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all products.</returns>
    Task<IEnumerable<Product>> GetAllAsync();

    /// <summary>
    /// Adds a new Product entity to the repository.
    /// </summary>
    /// <param name="product">The product entity to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Product product);

    // Potential future methods:
    // Task UpdateAsync(Product product);
    // Task DeleteAsync(Guid id);
}