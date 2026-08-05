using System.ComponentModel.DataAnnotations;

namespace PosBodega.Models;

public class PagoFiado
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.Now;

    public decimal Monto { get; set; }

    public decimal SaldoAnterior { get; set; }

    public decimal SaldoRestante { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }
}
