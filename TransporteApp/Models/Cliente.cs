using System.ComponentModel.DataAnnotations;

namespace TransporteApp.Models;

public record Cliente
{
    [Key]
    public int Id { get; init; }

    [Required]
    [StringLength(200)]
    public required string RazonSocial { get; init; }

    [StringLength(20)]
    public string? RUC { get; init; }

    public string? Direccion { get; init; }

    [StringLength(20)]
    public string? Telefono { get; init; }

    [StringLength(100)]
    public string? Email { get; init; }

    [StringLength(100)]
    public string? Contacto { get; init; }

    [Required]
    [StringLength(20)]
    public required string Estado { get; init; } = "Activo";

    public DateTime FechaCreacion { get; init; } = DateTime.Now;

    public DateTime FechaActualizacion { get; init; } = DateTime.Now;

    // Navegación
    public virtual ICollection<Viaje> Viajes { get; init; } = new List<Viaje>();
}



