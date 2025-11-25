using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteApp.Models;

public record Vehiculo
{
    [Key]
    public int Id { get; init; }

    [Required]
    [StringLength(10)]
    public required string Placa { get; set; }

    [Required]
    [StringLength(50)]
    public required string Marca { get; set; }

    [StringLength(50)]
    public string? Modelo { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public required decimal CapacidadLitros { get; set; }

    public int? AnioFabricacion { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaUltimoMantenimiento { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaVencimientoSeguro { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaVencimientoSoat { get; set; }

    [Required]
    [StringLength(20)]
    public required string Estado { get; set; } = "Activo";

    public string? Observaciones { get; init; }

    public DateTime FechaCreacion { get; init; } = DateTime.Now;

    public DateTime FechaActualizacion { get; init; } = DateTime.Now;

    // Navegación
    public virtual ICollection<Viaje> Viajes { get; init; } = new List<Viaje>();

    // Propiedades calculadas (no mapeadas a BD)
    [NotMapped]
    public bool SeguroPorVencer => FechaVencimientoSeguro.HasValue && 
                                    FechaVencimientoSeguro.Value <= DateTime.Now.AddDays(30) &&
                                    FechaVencimientoSeguro.Value > DateTime.Now;

    [NotMapped]
    public bool SeguroVencido => FechaVencimientoSeguro.HasValue && 
                                 FechaVencimientoSeguro.Value < DateTime.Now;
}

