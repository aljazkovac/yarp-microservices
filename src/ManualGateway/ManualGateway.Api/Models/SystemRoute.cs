namespace ManualGateway.Api.Models;

public class SystemRoute
{
    public string SystemId { get; init; } = string.Empty;
    public string ProductServiceTarget { get; init; } = string.Empty;
    public string InventoryServiceTarget { get; init; } = string.Empty; // We have this in DB, might use later
    public string OrderServiceTarget { get; init; } = string.Empty;   // We have this in DB, might use later
}