public class Empresa
{
    public int Id { get; set; }
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Description { get; set; }
    public string Alias { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Location { get; set; } = null!;
    public bool Active { get; set; }
    public object? Features { get; set; } // Adjust type to support JSON objects
    public string ResponsiblePerson { get; set; } = null!;
    public string ResponsibleEmail { get; set; } = null!;
}
