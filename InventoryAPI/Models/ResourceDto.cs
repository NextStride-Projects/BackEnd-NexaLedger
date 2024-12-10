public class ResourceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Features { get; set; }
    public string Category { get; set; }
    public bool Available { get; set; }
    public decimal Price { get; set; }
    public string Image { get; set; }
}