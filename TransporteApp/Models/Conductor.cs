using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteApp.Models;

public record Conductor
{
    [Key]
    public int Id { get; init; }

    [Required]
    [StringLength(20)]
    public required string NumeroDocumento { get; init; }

    [Required]
    [StringLength(100)]
    public required string Nombres { get; init; }

    [Required]
    [StringLength(100)]
    public required string Apellidos { get; init; }

    [StringLength(20)]
    public string? Telefono { get; init; }

    [StringLength(100)]
    public string? Email { get; init; }

    [Required]
    [StringLength(50)]
    public required string NumeroLicencia { get; init; }

    [Required]
    [Column(TypeName = "date")]
    public required DateTime FechaVencimientoLicencia { get; init; }

    public byte[]? FotoPerfil { get; init; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal CuentaCorriente { get; init; } = 0.00m;

    [Required]
    [StringLength(20)]
    public required string Estado { get; init; } = "Activo";

    public DateTime FechaCreacion { get; init; } = DateTime.Now;

    public DateTime FechaActualizacion { get; init; } = DateTime.Now;

    // Navegación
    public virtual ICollection<Viaje> Viajes { get; init; } = new List<Viaje>();
    public virtual ICollection<MovimientoCuentaCorriente> Movimientos { get; init; } = new List<MovimientoCuentaCorriente>();

    // Propiedades calculadas (no mapeadas a BD)
    [NotMapped]
    public string NombreCompleto => $"{Nombres} {Apellidos}";

    [NotMapped]
    public bool LicenciaPorVencer => FechaVencimientoLicencia <= DateTime.Now.AddDays(60) && 
                                      FechaVencimientoLicencia > DateTime.Now;

    [NotMapped]
    public bool LicenciaVencida => FechaVencimientoLicencia < DateTime.Now;
}

