using System.ComponentModel.DataAnnotations;

namespace PosBodega.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? Dni { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    public decimal SaldoDeudor { get; set; }

    public decimal LimiteCredito { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<PagoFiado> Pagos { get; set; } = new List<PagoFiado>();
}
