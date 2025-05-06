namespace OrderService.Domain;

/// <summary>
/// Defines the contract for repository operations related to the Order aggregate root.
/// Resides in the Domain layer; implementation will be in the Infrastructure layer.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves an Order entity by its unique identifier.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the order or null if not found.</returns>
    Task<Order?> GetByIdAsync(Guid orderId);

    /// <summary>
    /// Retrieves all Order entities.
    /// Consider adding pagination parameters for real-world scenarios.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The result contains a collection of all orders.</returns>
    Task<IEnumerable<Order>> GetAllAsync();

    /// <summary>
    /// Retrieves all orders placed by a specific customer.
    /// Consider adding pagination parameters for real-world scenarios.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A task representing the asynchronous operation. The result contains a collection of orders for the specified customer.</returns>
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);

    /// <summary>
    /// Adds a new Order entity to the repository.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Order order);

    /// <summary>
    /// Updates an existing Order entity in the repository.
    /// Used to persist changes to the aggregate root and its contained items.
    /// </summary>
    /// <param name="order">The order with updated state.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Order order);

    // Potential future methods:
    // Task DeleteAsync(Guid orderId);
}