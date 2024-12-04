using Microsoft.EntityFrameworkCore;
using RecorderAPI.Models;

namespace RecorderAPI.Data
{
    public class LogContext : DbContext
    {
        public LogContext(DbContextOptions<LogContext> options)
            : base(options) { }

        public DbSet<Log> Logs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.EmpresaId).IsRequired();
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
