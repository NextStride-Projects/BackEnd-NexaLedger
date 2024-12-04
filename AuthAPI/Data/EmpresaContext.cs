using AuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Data
{
    public class EmpresaContext : DbContext
    {
        public EmpresaContext(DbContextOptions<EmpresaContext> options)
            : base(options) { }

        public DbSet<Empresa> Empresas { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.ToTable("Empresas");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Direccion).HasMaxLength(255);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);

                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.Alias).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).IsRequired().HasColumnType("varchar(20)");
                entity.Property(e => e.Location).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Active).IsRequired();
                entity.Property(e => e.Features).HasColumnType("jsonb");
                entity.Property(e => e.ResponsiblePerson).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ResponsibleEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StaffCount).IsRequired().HasDefaultValue(0);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(255);

                entity
                    .HasOne<Empresa>()
                    .WithMany()
                    .HasForeignKey(u => u.EmpresaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
