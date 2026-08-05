using System.ComponentModel.DataAnnotations;

namespace PosBodega.Models;

public class Compra
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public decimal Total { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public ICollection<CompraDetalle> Detalles { get; set; } = new List<CompraDetalle>();
}
