using InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Data;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<Movement> Movements { get; set; }
}
