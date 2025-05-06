namespace OrderService.Domain;

/// <summary>
/// Represents an item within an order.
/// In DDD, this can be an Entity if it has its own lifecycle or identity within the Order aggregate,
/// or a Value Object if defined purely by its attributes.
/// For now, we give it an Id, treating it as an entity local to the Order.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Unique identifier for the order item (local to the order).
    /// </summary>
    public Guid OrderItemId { get; private set; }

    /// <summary>
    /// The ID of the product being ordered.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// The name of the product at the time the order was placed.
    /// Stored to avoid changes if the product name changes in the ProductService later.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// The price of a single unit of the product at the time the order was placed.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// The number of units of this product ordered.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Calculated total for this line item.
    /// </summary>
    public decimal TotalPrice => UnitPrice * Quantity;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    private OrderItem() { } // For ORM
#pragma warning restore CS8618

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));
        if (unitPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be positive.");
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        OrderItemId = Guid.NewGuid(); // Generate local ID
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    // Potential future methods:
    // internal void UpdateQuantity(int newQuantity) { ... Quantity = newQuantity; }
}