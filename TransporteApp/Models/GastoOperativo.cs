using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransporteApp.Models;

public record GastoOperativo
{
    [Key]
    public int Id { get; init; }

    [Required]
    public required int ViajeId { get; set; }

    [Required]
    [StringLength(20)]
    public required string Tipo { get; set; } // Renamed from Categoria to match requirements

    [Required]
    [StringLength(200)]
    public required string Descripcion { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public required decimal Monto { get; set; }

    [Required]
    public required DateTime Fecha { get; set; } // Renamed from FechaGasto

    public byte[]? Comprobante { get; set; }

    public DateTime FechaCreacion { get; init; } = DateTime.Now;

    // Navegación
    public virtual Viaje Viaje { get; init; } = null!;
}
