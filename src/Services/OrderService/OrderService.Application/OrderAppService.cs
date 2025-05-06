using OrderService.Domain; // Needs reference to Domain project

namespace OrderService.Application;

/// <summary>
/// Application service for handling order-related use cases.
/// </summary>
public class OrderAppService
{
    private readonly IOrderRepository _orderRepository;
    // In a real system, this service might also need to interact with:
    // - IProductService (to get product details like current name and price)
    // - IInventoryService (to check stock and reserve items)
    // For now, we'll assume Product Name and Price are passed in, and skip inventory checks.

    public OrderAppService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="request">The request DTO containing order details.</param>
    /// <returns>The DTO of the newly created order.</returns>
    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Order must contain at least one item.", nameof(request.Items));

        // Create the domain Order aggregate root
        var order = new Order(request.CustomerId); // OrderId is generated in constructor

        // Add items to the order
        // In a real system, you'd fetch product details (name, current price) from ProductService
        // and potentially check inventory with InventoryService here.
        foreach (var itemRequest in request.Items)
        {
            // For now, we trust the ProductName and UnitPrice from the request.
            // This is a simplification.
            order.AddItem(
                itemRequest.ProductId,
                itemRequest.ProductName,
                itemRequest.UnitPrice,
                itemRequest.Quantity
            );
        }

        // Persist the new order
        await _orderRepository.AddAsync(order);

        // Map to DTO and return
        return MapOrderToDto(order);
    }

    /// <summary>
    /// Gets an order by its ID.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>The OrderDto or null if not found.</returns>
    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order == null ? null : MapOrderToDto(order);
    }

    /// <summary>
    /// Gets all orders for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>A collection of OrderDtos.</returns>
    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return orders.Select(MapOrderToDto);
    }

    /// <summary>
    /// Confirms an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to confirm.</param>
    /// <returns>The updated OrderDto.</returns>
    /// <exception cref="InvalidOperationException">If order not found or cannot be confirmed.</exception>
    public async Task<OrderDto> ConfirmOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {orderId} not found.");
        }

        order.Confirm(); // Domain logic for confirmation
        await _orderRepository.UpdateAsync(order); // Persist the status change

        return MapOrderToDto(order);
    }


    // --- Private Helper Methods for Mapping ---
    private OrderDto MapOrderToDto(Order order)
    {
        return new OrderDto(
            order.OrderId,
            order.CustomerId,
            order.OrderDate,
            order.Status.ToString(), // Convert enum to string for DTO
            order.OrderItems.Select(MapOrderItemToDto).ToList(),
            order.TotalPrice
        );
    }

    private OrderItemDto MapOrderItemToDto(OrderItem item)
    {
        return new OrderItemDto(
            item.OrderItemId,
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity,
            item.TotalPrice
        );
    }
}