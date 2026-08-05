using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class ReporteService
{
    private readonly AppDbContext _db;

    public ReporteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ResumenDiario> GetResumenDiarioAsync()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        var ventasHoy = await _db.Ventas
            .Where(v => v.Fecha >= hoy && v.Fecha < manana)
            .ToListAsync();

        var totalFiados = await _db.Clientes
            .Where(c => c.Activo && c.SaldoDeudor > 0)
            .SumAsync(c => c.SaldoDeudor);

        var productosStockBajo = await _db.Productos
            .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
            .CountAsync();

        return new ResumenDiario
        {
            VentasDelDia = ventasHoy.Count,
            TotalVentasDelDia = ventasHoy.Sum(v => v.Total),
            TotalFiados = totalFiados,
            ProductosStockBajo = productosStockBajo
        };
    }

    public async Task<List<Venta>> GetVentasPorRangoAsync(DateTime desde, DateTime hasta)
    {
        return await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.Fecha >= desde && v.Fecha <= hasta.AddDays(1))
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<List<ProductoVendido>> GetProductosMasVendidosAsync(DateTime desde, DateTime hasta)
    {
        return await _db.VentaDetalles
            .Include(d => d.Producto)
            .Include(d => d.Venta)
            .Where(d => d.Venta.Fecha >= desde && d.Venta.Fecha <= hasta.AddDays(1))
            .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
            .Select(g => new ProductoVendido
            {
                ProductoNombre = g.Key.Nombre,
                CantidadVendida = g.Sum(d => d.Cantidad),
                TotalVendido = g.Sum(d => d.Subtotal)
            })
            .OrderByDescending(p => p.CantidadVendida)
            .Take(20)
            .ToListAsync();
    }

    public async Task<GananciaBruta> GetGananciaBrutaAsync(DateTime desde, DateTime hasta)
    {
        var ventas = await _db.VentaDetalles
            .Include(d => d.Producto)
            .Include(d => d.Venta)
            .Where(d => d.Venta.Fecha >= desde && d.Venta.Fecha <= hasta.AddDays(1))
            .ToListAsync();

        var ingresos = ventas.Sum(d => d.Subtotal);
        var costos = ventas.Sum(d => d.Cantidad * d.Producto.Costo);

        return new GananciaBruta
        {
            Ingresos = ingresos,
            Costos = costos,
            Ganancia = ingresos - costos
        };
    }
}

// DTOs for reports
public class ResumenDiario
{
    public int VentasDelDia { get; set; }
    public decimal TotalVentasDelDia { get; set; }
    public decimal TotalFiados { get; set; }
    public int ProductosStockBajo { get; set; }
}

public class ProductoVendido
{
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
}

public class GananciaBruta
{
    public decimal Ingresos { get; set; }
    public decimal Costos { get; set; }
    public decimal Ganancia { get; set; }
}
