using System.Collections.Concurrent;
using OrderService.Domain; // Need reference to Domain project

namespace OrderService.Infrastructure;

/// <summary>
/// In-memory implementation of the IOrderRepository.
/// Lives in the Infrastructure layer.
/// </summary>
public class InMemoryOrderRepository : IOrderRepository
{
    // Using OrderId as the key for the dictionary.
    private static readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task<Order?> GetByIdAsync(Guid orderId)
    {
        _orders.TryGetValue(orderId, out var order);
        return Task.FromResult(order); // Simulate async
    }

    public Task<IEnumerable<Order>> GetAllAsync()
    {
        return Task.FromResult(_orders.Values.AsEnumerable()); // Simulate async
    }

    public Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        var customerOrders = _orders.Values.Where(o => o.CustomerId == customerId).ToList();
        return Task.FromResult(customerOrders.AsEnumerable()); // Simulate async
    }

    public Task AddAsync(Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }
        _orders.TryAdd(order.OrderId, order);
        return Task.CompletedTask; // Simulate async
    }

    public Task UpdateAsync(Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        if (_orders.ContainsKey(order.OrderId))
        {
            _orders[order.OrderId] = order; // Simple replace for in-memory
        }
        else
        {
            // Or throw if order must exist to be updated
            _orders.TryAdd(order.OrderId, order);
        }
        return Task.CompletedTask; // Simulate async
    }
}