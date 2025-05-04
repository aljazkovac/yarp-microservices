using System.Collections.Concurrent;
using ProductService.Domain; // Need to reference the Domain project

namespace ProductService.Infrastructure;

/// <summary>
/// An in-memory implementation of the IProductRepository for testing and development purposes.
/// This class resides in the Infrastructure layer as it provides a concrete data access mechanism.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    // Using ConcurrentDictionary for basic thread safety, suitable for simple scenarios.
    // Marked static so the data persists across different instantiations of the repository (within the same app lifetime).
    private static readonly ConcurrentDictionary<Guid, Product> _products = new();

    /// <summary>
    /// Retrieves a Product entity by its unique identifier from the in-memory store.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the product entity or null if not found.</returns>
    public Task<Product?> GetByIdAsync(Guid id)
    {
        _products.TryGetValue(id, out var product);
        // Simulate async operation
        return Task.FromResult(product);
    }

    /// <summary>
    /// Retrieves all Product entities from the in-memory store.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all products.</returns>
    public Task<IEnumerable<Product>> GetAllAsync()
    {
        // Simulate async operation
        return Task.FromResult(_products.Values.AsEnumerable());
    }

    /// <summary>
    /// Adds a new Product entity to the in-memory store.
    /// </summary>
    /// <param name="product">The product entity to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddAsync(Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        _products.TryAdd(product.Id, product);
        // Simulate async operation
        return Task.CompletedTask;
    }

    // Note: Implementation for UpdateAsync/DeleteAsync would involve _products.TryUpdate/TryRemove
}