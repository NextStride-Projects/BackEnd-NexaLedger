namespace InventoryAPI.Models;

public class Resource
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Features { get; set; } = new();
    public string Category { get; set; }
    public bool Available { get; set; }
    public bool SaleAvailability { get; set; }
    public decimal Price { get; set; }
    public double Size { get; set; }
    public string Image { get; set; }
}