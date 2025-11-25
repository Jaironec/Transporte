using Microsoft.EntityFrameworkCore;
using TransporteApp.Models;

namespace TransporteApp.Data;

public class TransporteDbContext : DbContext
{
    public TransporteDbContext(DbContextOptions<TransporteDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehiculo> Vehiculos { get; set; } = null!;
    public DbSet<Conductor> Conductores { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Viaje> Viajes { get; set; } = null!;
    public DbSet<GastoOperativo> GastosOperativos { get; set; } = null!;
    public DbSet<MovimientoCuentaCorriente> MovimientosCuentaCorriente { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar nombres de tablas explícitamente (PostgreSQL es case-sensitive)
        modelBuilder.Entity<Vehiculo>().ToTable("Vehiculos");
        modelBuilder.Entity<Conductor>().ToTable("Conductores");
        modelBuilder.Entity<Cliente>().ToTable("Clientes");
        modelBuilder.Entity<Viaje>().ToTable("Viajes");
        modelBuilder.Entity<GastoOperativo>().ToTable("Gastos"); // Keep table name as 'Gastos' or change to 'GastosOperativos'? User said "GastoOperativo: ID..." but didn't specify table name change, but usually implies it. Let's keep it mapped to "Gastos" for now to avoid breaking SQL script too much, or update SQL script. Actually, I should update SQL script to be consistent. Let's map to "GastosOperativos".
        modelBuilder.Entity<MovimientoCuentaCorriente>().ToTable("MovimientosCuentaCorriente");

        // ... (Vehiculo, Conductor, Cliente, Viaje config remains same) ...

        // Configuración de Vehiculo
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasIndex(e => e.Placa).IsUnique();
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.FechaVencimientoSeguro);
        });

        // Configuración de Conductor
        modelBuilder.Entity<Conductor>(entity =>
        {
            entity.HasIndex(e => e.NumeroDocumento).IsUnique();
            entity.HasIndex(e => e.NumeroLicencia);
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.FechaVencimientoLicencia);
        });

        // Configuración de Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(e => e.RUC);
            entity.HasIndex(e => e.Estado);
        });

        // Configuración de Viaje
        modelBuilder.Entity<Viaje>(entity =>
        {
            entity.HasIndex(e => e.NumeroViaje).IsUnique();
            entity.HasIndex(e => e.FechaSalida);
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.VehiculoId);
            entity.HasIndex(e => e.ConductorId);
            entity.HasIndex(e => e.ClienteId);

            entity.HasOne(v => v.Vehiculo)
                  .WithMany(v => v.Viajes)
                  .HasForeignKey(v => v.VehiculoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Conductor)
                  .WithMany(c => c.Viajes)
                  .HasForeignKey(v => v.ConductorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Cliente)
                  .WithMany(c => c.Viajes)
                  .HasForeignKey(v => v.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de GastoOperativo
        modelBuilder.Entity<GastoOperativo>(entity =>
        {
            entity.ToTable("Gastos"); // Mapping to existing table "Gastos" to avoid SQL script rewrite if possible, but properties changed (Categoria -> Tipo, FechaGasto -> Fecha). I need to update SQL script anyway.
            entity.HasIndex(e => e.ViajeId);
            entity.HasIndex(e => e.Tipo);
            entity.HasIndex(e => e.Fecha);

            entity.HasOne(g => g.Viaje)
                  .WithMany(v => v.Gastos)
                  .HasForeignKey(g => g.ViajeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de MovimientoCuentaCorriente
        modelBuilder.Entity<MovimientoCuentaCorriente>(entity =>
        {
            entity.HasIndex(e => e.ConductorId);
            entity.HasIndex(e => e.FechaMovimiento);
            entity.HasIndex(e => e.ViajeId);

            entity.HasOne(m => m.Conductor)
                  .WithMany(c => c.Movimientos)
                  .HasForeignKey(m => m.ConductorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Viaje)
                  .WithMany()
                  .HasForeignKey(m => m.ViajeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}



