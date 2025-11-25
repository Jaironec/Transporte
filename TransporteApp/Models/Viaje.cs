using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TransporteApp.Models;

public record Viaje
{
    [Key]
    public int Id { get; init; }

    [Required]
    [StringLength(20)]
    public required string NumeroViaje { get; set; }

    [Required]
    public required int VehiculoId { get; set; }

    [Required]
    public required int ConductorId { get; set; }

    [Required]
    public required int ClienteId { get; set; }

    [Required]
    public required DateTime FechaSalida { get; set; }

    public DateTime? FechaLlegada { get; set; }

    [Required]
    [StringLength(200)]
    public required string Origen { get; set; }

    [Required]
    [StringLength(200)]
    public required string Destino { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public required decimal CantidadLitros { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public required decimal Flete { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public required decimal PagoConductor { get; set; }

    [Required]
    [StringLength(20)]
    public required string Estado { get; set; } = "Programado";

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime FechaActualizacion { get; set; } = DateTime.Now;

    // Navegación
    public virtual Vehiculo Vehiculo { get; init; } = null!;
    public virtual Conductor Conductor { get; init; } = null!;
    public virtual Cliente Cliente { get; init; } = null!;
    public virtual ICollection<GastoOperativo> Gastos { get; init; } = new List<GastoOperativo>();

    // Propiedades calculadas (no mapeadas a BD)
    [NotMapped]
    public decimal UtilidadBruta => Flete - PagoConductor;

    [NotMapped]
    public decimal TotalGastos => Gastos?.Sum(g => g.Monto) ?? 0m;

    [NotMapped]
    public decimal UtilidadNeta => UtilidadBruta - TotalGastos;
}

