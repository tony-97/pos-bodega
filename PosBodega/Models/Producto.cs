using System.ComponentModel.DataAnnotations;

namespace PosBodega.Models;

public class Producto
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public UnidadMedida UnidadMedida { get; set; } = UnidadMedida.Unidad;

    [MaxLength(50)]
    public string? CodigoBarras { get; set; }

    public decimal Costo { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal StockActual { get; set; }

    public decimal StockMinimo { get; set; } = 5;

    public DateTime? FechaVencimiento { get; set; }

    public bool Activo { get; set; } = true;

    // Helper properties for UI & business logic
    public bool EsPerecible => FechaVencimiento.HasValue;
    public bool EstaVencido => FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today;
    public bool PorVencer => FechaVencimiento.HasValue && FechaVencimiento.Value.Date >= DateTime.Today && FechaVencimiento.Value.Date <= DateTime.Today.AddDays(15);
    public int? DiasParaVencer => FechaVencimiento.HasValue ? (FechaVencimiento.Value.Date - DateTime.Today).Days : null;
}

