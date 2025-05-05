using System.Collections.Concurrent;
using InventoryService.Domain; // Need reference to Domain project

namespace InventoryService.Infrastructure;

/// <summary>
/// An in-memory implementation of the IInventoryRepository.
/// Lives in the Infrastructure layer.
/// </summary>
public class InMemoryInventoryRepository : IInventoryRepository
{
    // Using ProductId as the key for the dictionary.
    private static readonly ConcurrentDictionary<Guid, InventoryItem> _inventory = new();

    /// <summary>
    /// Retrieves an InventoryItem entity by its associated ProductId from the in-memory store.
    /// </summary>
    public Task<InventoryItem?> GetByProductIdAsync(Guid productId)
    {
        _inventory.TryGetValue(productId, out var item);
        // Simulate async operation
        return Task.FromResult(item);
    }

    /// <summary>
    /// Retrieves all InventoryItem entities from the in-memory store.
    /// </summary>
    public Task<IEnumerable<InventoryItem>> GetAllAsync()
    {
        // Simulate async operation
        return Task.FromResult(_inventory.Values.AsEnumerable());
    }

    /// <summary>
    /// Adds a new InventoryItem entity to the in-memory store.
    /// </summary>
    public Task AddAsync(InventoryItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        _inventory.TryAdd(item.ProductId, item);
        // Simulate async operation
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates an existing InventoryItem entity in the in-memory store.
    /// In a real DB, this would perform an UPDATE operation. Here, we replace the entry.
    /// Note: ConcurrentDictionary's Update/AddOrUpdate could also be used for more atomicity.
    /// </summary>
    public Task UpdateAsync(InventoryItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        // Simple replace strategy for in-memory
        if (_inventory.ContainsKey(item.ProductId))
        {
            _inventory[item.ProductId] = item;
        }
        else
        {
            // Or throw an exception if item must exist to be updated
            _inventory.TryAdd(item.ProductId, item);
        }

        // Simulate async operation
        return Task.CompletedTask;
    }
}