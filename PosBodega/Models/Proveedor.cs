using System.ComponentModel.DataAnnotations;

namespace PosBodega.Models;

public class Proveedor
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Contacto { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    public bool Activo { get; set; } = true;
}
