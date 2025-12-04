using GDT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GDT.Infrastructure.Data;

public class GdtDbContext : DbContext
{
    public GdtDbContext(DbContextOptions<GdtDbContext> options) : base(options)
    {
    }

    public DbSet<Declaracion> Declaraciones => Set<Declaracion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Declaracion>(entity =>
        {
            entity.ToTable("Declaraciones");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.NumeroDeclaracion)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.RNC)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.RazonSocial)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.TipoImpuesto)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.MontoIngresos)
                .HasPrecision(18, 2);

            entity.Property(e => e.MontoGastos)
                .HasPrecision(18, 2);

            entity.Property(e => e.ImpuestoCalculado)
                .HasPrecision(18, 2);

            entity.Property(e => e.Sancion)
                .HasPrecision(18, 2);

            entity.Property(e => e.Estado)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.FechaCreacion)
                .IsRequired();

            entity.Property(e => e.ObservacionesRechazo)
                .HasMaxLength(500);

            // Índices para mejorar performance de consultas
            entity.HasIndex(e => e.NumeroDeclaracion)
                .IsUnique();

            entity.HasIndex(e => new { e.RNC, e.Periodo, e.TipoImpuesto })
                .IsUnique();

            entity.HasIndex(e => e.RNC);

            entity.HasIndex(e => e.Estado);
        });
    }
}
