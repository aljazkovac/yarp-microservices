using InventoryService.Domain; // Need reference to Domain project

namespace InventoryService.Application;

/// <summary>
/// Application service for handling inventory-related use cases.
/// </summary>
public class InventoryAppService
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryAppService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
    }

    /// <summary>
    /// Gets the inventory status for a specific product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The InventoryItemDto or null if not found.</returns>
    public async Task<InventoryItemDto?> GetInventoryByProductIdAsync(Guid productId)
    {
        var item = await _inventoryRepository.GetByProductIdAsync(productId);
        if (item == null)
        {
            return null;
        }

        return MapToDto(item);
    }

    /// <summary>
    /// Gets inventory status for all tracked products.
    /// </summary>
    /// <returns>A collection of InventoryItemDtos.</returns>
    public async Task<IEnumerable<InventoryItemDto>> GetAllInventoryAsync()
    {
        var items = await _inventoryRepository.GetAllAsync();
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Increases the stock for a product. Creates the item if it doesn't exist.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="amount">The positive amount to increase by.</param>
    /// <returns>The updated InventoryItemDto.</returns>
    public async Task<InventoryItemDto> IncreaseStockAsync(Guid productId, int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

        var item = await _inventoryRepository.GetByProductIdAsync(productId);

        if (item == null)
        {
            // If item doesn't exist, create it
            item = new InventoryItem(productId, amount);
            await _inventoryRepository.AddAsync(item);
        }
        else
        {
            // Otherwise, increase stock using the domain object method
            item.IncreaseStock(amount);
            await _inventoryRepository.UpdateAsync(item);
        }

        return MapToDto(item);
    }

     /// <summary>
    /// Decreases the stock for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="amount">The positive amount to decrease by.</param>
    /// <returns>The updated InventoryItemDto.</returns>
    /// <exception cref="InvalidOperationException">Thrown if stock is insufficient or item not found.</exception>
     /// <exception cref="ArgumentOutOfRangeException">Thrown if amount is not positive.</exception>
    public async Task<InventoryItemDto> DecreaseStockAsync(Guid productId, int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

        var item = await _inventoryRepository.GetByProductIdAsync(productId);

        if (item == null)
        {
            // Or maybe create it with negative stock if business rules allow? For now, throw.
            throw new InvalidOperationException($"Inventory item not found for ProductId: {productId}");
        }

        // Decrease stock using the domain object method, which checks for sufficiency
        bool success = item.DecreaseStock(amount);
        if (!success)
        {
            throw new InvalidOperationException($"Insufficient stock for ProductId: {productId}. Available: {item.QuantityOnHand}, Requested: {amount}");
        }

        await _inventoryRepository.UpdateAsync(item);
        return MapToDto(item);
    }

    /// <summary>
    /// Sets the stock for a product directly. Creates the item if it doesn't exist.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="quantity">The new quantity.</param>
    /// <returns>The updated InventoryItemDto.</returns>
    public async Task<InventoryItemDto> SetStockAsync(Guid productId, int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

        var item = await _inventoryRepository.GetByProductIdAsync(productId);

        if (item == null)
        {
            item = new InventoryItem(productId, quantity);
            await _inventoryRepository.AddAsync(item);
        }
        else
        {
            item.SetStock(quantity);
            await _inventoryRepository.UpdateAsync(item);
        }
        return MapToDto(item);
    }


    // Simple private helper method for mapping
    private InventoryItemDto MapToDto(InventoryItem item)
    {
        return new InventoryItemDto(
            item.ProductId,
            item.QuantityOnHand,
            item.LastUpdated
        );
    }
}