namespace Gateway.Api.Models;

public class SystemRoute
{
    public string SystemId { get; set; } = string.Empty;
    public string ProductServiceTarget { get; set; } = string.Empty;
    public string InventoryServiceTarget { get; set; } = string.Empty; // We have this in DB, might use later
    public string OrderServiceTarget { get; set; } = string.Empty;   // We have this in DB, might use later
}