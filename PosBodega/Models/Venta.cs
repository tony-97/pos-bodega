using System.ComponentModel.DataAnnotations;

namespace PosBodega.Models;

public class Venta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [MaxLength(10)]
    public string Serie { get; set; } = "B001";

    public int Numero { get; set; } = 1;

    [MaxLength(20)]
    public string NumeroComprobante { get; set; } = "B001-000001";

    public TipoComprobante TipoComprobante { get; set; } = TipoComprobante.Boleta;

    public EstadoVenta Estado { get; set; } = EstadoVenta.Emitida;

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [MaxLength(200)]
    public string ClienteNombre { get; set; } = "Clientes Varios";

    public TipoDocumentoIdentidad ClienteTipoDocumento { get; set; } = TipoDocumentoIdentidad.SinDocumento;

    [MaxLength(20)]
    public string? ClienteDocumento { get; set; }

    [MaxLength(300)]
    public string? ClienteDireccion { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public MetodoPago MetodoPago { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Igv { get; set; }

    public decimal Total { get; set; }

    public decimal? MontoRecibido { get; set; }

    public decimal? Vuelto { get; set; }

    public bool EsFiado { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public int? TicketOrigenId { get; set; }

    public int? ComprobanteDestinoId { get; set; }

    [MaxLength(300)]
    public string? MotivoAnulacion { get; set; }

    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
}

