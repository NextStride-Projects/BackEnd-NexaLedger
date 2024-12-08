namespace InventoryAPI.Models;

public class Movement
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public string UserId { get; set; }
    public string Type { get; set; } // Example: "Addition", "Removal", "Update"
    public string Description { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
