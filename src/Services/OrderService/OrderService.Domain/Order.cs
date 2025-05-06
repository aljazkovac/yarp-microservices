namespace OrderService.Domain;

/// <summary>
/// Represents a customer's order.
/// This is the Aggregate Root for the Order Aggregate. It manages the collection of OrderItems
/// and ensures the consistency of the order as a whole.
/// </summary>
public class Order
{
    /// <summary>
    /// Unique identifier for the order.
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// Identifier for the customer who placed the order.
    /// For simplicity, we're using a Guid without a full Customer entity.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// The date and time when the order was placed.
    /// </summary>
    public DateTimeOffset OrderDate { get; private set; }

    /// <summary>
    /// The current status of the order.
    /// </summary>
    public OrderStatus Status { get; private set; } // Could be an enum: Pending, Confirmed, Shipped, Cancelled

    // Private backing field for the items.
    // The Aggregate Root controls access to this collection.
    private readonly List<OrderItem> _orderItems = new();

    /// <summary>
    /// Gets the list of items in this order.
    /// Exposing as IReadOnlyList to prevent external modification of the collection directly.
    /// Changes should go through methods on the Order Aggregate Root.
    /// </summary>
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    /// <summary>
    /// Calculates the total price of the order.
    /// </summary>
    public decimal TotalPrice => OrderItems.Sum(item => item.TotalPrice);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    private Order() { } // For ORM
#pragma warning restore CS8618

    public Order(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));

        OrderId = Guid.NewGuid();
        CustomerId = customerId;
        OrderDate = DateTimeOffset.UtcNow;
        Status = OrderStatus.Pending; // Default status
    }

    /// <summary>
    /// Adds an item to the order.
    /// This method is part of the Aggregate Root's responsibility to manage its internal state.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="productName">The product name.</param>
    /// <param name="unitPrice">The price per unit.</param>
    /// <param name="quantity">The quantity.</param>
    /// <exception cref="InvalidOperationException">Thrown if trying to add items to a non-pending order.</exception>
    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to an order that is not in Pending state.");

        var existingItem = _orderItems.FirstOrDefault(oi => oi.ProductId == productId);
        if (existingItem != null)
        {
            // Option 1: Update quantity (requires OrderItem.UpdateQuantity method)
            // existingItem.UpdateQuantity(existingItem.Quantity + quantity);

            // Option 2: Throw exception, disallowing duplicate product lines
             throw new InvalidOperationException($"Product {productId} already exists in this order. Modify existing item instead.");

            // Option 3: Just add another line item
            // _orderItems.Add(new OrderItem(productId, productName, unitPrice, quantity));
        }
        _orderItems.Add(new OrderItem(productId, productName, unitPrice, quantity));
        // Note: Recalculation of TotalPrice is implicit via the property.
    }

    /// <summary>
    /// Confirms the order.
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only a pending order can be confirmed.");
        if (!OrderItems.Any())
            throw new InvalidOperationException("Cannot confirm an empty order.");

        Status = OrderStatus.Confirmed;
        // Potentially raise a Domain Event here: OrderConfirmedEvent
    }

    // Other methods like Ship(), Cancel() could be added later
    // and would also change the Status and enforce rules.
}

/// <summary>
/// Represents the possible statuses of an order.
/// </summary>
public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}