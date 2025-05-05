namespace InventoryService.Domain;

/// <summary>
/// Defines the contract for repository operations related to the InventoryItem aggregate root.
/// Resides in the Domain layer, implementation is in the Infrastructure layer.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Retrieves an InventoryItem entity by its associated ProductId.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the inventory item or null if not found.</returns>
    Task<InventoryItem?> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Retrieves all InventoryItem entities.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The result contains a collection of all inventory items.</returns>
    Task<IEnumerable<InventoryItem>> GetAllAsync();

    /// <summary>
    /// Adds a new InventoryItem entity to the repository.
    /// Typically used when tracking a product for the first time.
    /// </summary>
    /// <param name="item">The inventory item to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(InventoryItem item);

    /// <summary>
    /// Updates an existing InventoryItem entity in the repository.
    /// Used to persist changes made to the aggregate (e.g., quantity changes).
    /// </summary>
    /// <param name="item">The inventory item with updated state.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(InventoryItem item);

    // Potential future methods:
    // Task DeleteAsync(Guid productId);
    // Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int threshold);
}