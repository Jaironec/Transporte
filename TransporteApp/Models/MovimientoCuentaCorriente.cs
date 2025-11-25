using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteApp.Models;

public record MovimientoCuentaCorriente
{
    [Key]
    public int Id { get; init; }

    [Required]
    public required int ConductorId { get; init; }

    public int? ViajeId { get; init; }

    [Required]
    [StringLength(20)]
    public required string Tipo { get; init; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public required decimal Monto { get; init; }

    [Required]
    [StringLength(200)]
    public required string Descripcion { get; init; }

    public DateTime FechaMovimiento { get; init; } = DateTime.Now;

    // Navegación
    public virtual Conductor Conductor { get; init; } = null!;
    public virtual Viaje? Viaje { get; init; }
}



