namespace InventoryService.Domain;

/// <summary>
/// Represents the inventory status for a specific product.
/// In DDD terms, InventoryItem is the Aggregate Root for the Inventory Aggregate.
/// It encapsulates the quantity on hand and enforces consistency rules (invariants).
/// The ProductId acts as the unique identifier for this aggregate root.
/// </summary>
public class InventoryItem
{
    /// <summary>
    /// Gets the unique identifier of the product associated with this inventory item.
    /// This serves as the primary key/identity for the InventoryItem aggregate root.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the current quantity of the product available in stock.
    /// </summary>
    public int QuantityOnHand { get; set; }

    /// <summary>
    /// Gets the timestamp of the last update to this inventory item.
    /// </summary>
    public DateTimeOffset LastUpdated { get; private set; }

    /// <summary>
    /// Private constructor for ORM/serialization frameworks.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private InventoryItem() { }
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new inventory item record.
    /// Ensures initial quantity is not negative.
    /// </summary>
    /// <param name="productId">The unique product identifier.</param>
    /// <param name="initialQuantity">The starting quantity.</param>
    public InventoryItem(Guid productId, int initialQuantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        if (initialQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialQuantity), "Initial quantity cannot be negative."); // Enforcing invariant

        ProductId = productId;
        QuantityOnHand = initialQuantity;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Increases the stock quantity.
    /// This method encapsulates the state change logic for the aggregate root.
    /// </summary>
    /// <param name="amount">The positive amount to increase by.</param>
    public void IncreaseStock(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount to increase must be positive.");

        QuantityOnHand += amount;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Decreases the stock quantity.
    /// Enforces the invariant that stock cannot go below zero.
    /// </summary>
    /// <param name="amount">The positive amount to decrease by.</param>
    /// <returns>True if stock was sufficient and decreased; false otherwise.</returns>
    public bool DecreaseStock(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount to decrease must be positive.");

        if (QuantityOnHand < amount)
        {
            // Not enough stock, invariant prevents going negative.
            // Alternatively, could throw an exception depending on business requirements.
            return false;
        }

        QuantityOnHand -= amount;
        LastUpdated = DateTimeOffset.UtcNow;
        return true;
    }

    /// <summary>
    /// Sets the stock quantity directly. Use with caution.
    /// Primarily useful for initial seeding or reconciliation tasks.
    /// </summary>
    /// <param name="newQuantity">The new stock quantity.</param>
    public void SetStock(int newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity cannot be negative."); // Enforcing invariant

        QuantityOnHand = newQuantity;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}