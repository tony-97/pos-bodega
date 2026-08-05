using System.ComponentModel.DataAnnotations;

namespace PosBodega.Models;

public class Venta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public MetodoPago MetodoPago { get; set; }

    public TipoComprobante TipoComprobante { get; set; } = TipoComprobante.TicketInterno;

    public decimal Total { get; set; }

    public bool EsFiado { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
}
