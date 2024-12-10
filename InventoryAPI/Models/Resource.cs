namespace InventoryAPI.Models;

public class Resource
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<string> Features { get; set; } = new();
    public required string Category { get; set; }
    public bool Available { get; set; }
    public bool SaleAvailability { get; set; }
    public decimal Price { get; set; }
    public double Size { get; set; }
    public string Image { get; set; }
    public DateTime AcquiredAt { get; set; }
    public DateTime? LatestMovementDate { get; set; }
}