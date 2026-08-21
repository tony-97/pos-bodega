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

        var ventasValidas = ventasHoy.Where(v => v.Estado == EstadoVenta.Emitida).ToList();
        var devolucionesHoy = ventasHoy.Where(v => v.Estado == EstadoVenta.Anulada).ToList();

        var totalFiados = await _db.Clientes
            .Where(c => c.Activo && c.SaldoDeudor > 0)
            .SumAsync(c => c.SaldoDeudor);

        var productosStockBajo = await _db.Productos
            .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
            .CountAsync();

        var limitVencimiento = hoy.AddDays(15);
        var productosVencidos = await _db.Productos
            .Where(p => p.Activo && p.FechaVencimiento != null && p.FechaVencimiento < hoy && p.StockActual > 0)
            .CountAsync();

        var productosPorVencer = await _db.Productos
            .Where(p => p.Activo && p.FechaVencimiento != null && p.FechaVencimiento >= hoy && p.FechaVencimiento <= limitVencimiento && p.StockActual > 0)
            .CountAsync();

        return new ResumenDiario
        {
            VentasDelDia = ventasValidas.Count,
            TotalVentasDelDia = ventasValidas.Sum(v => v.Total),
            DevolucionesDelDia = devolucionesHoy.Count,
            TotalDevolucionesDelDia = devolucionesHoy.Sum(v => v.Total),
            TotalFiados = totalFiados,
            ProductosStockBajo = productosStockBajo,
            ProductosPorVencer = productosPorVencer,
            ProductosVencidos = productosVencidos
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

    public async Task<List<Venta>> GetDevolucionesPorRangoAsync(DateTime desde, DateTime hasta)
    {
        return await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado == EstadoVenta.Anulada)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<ResumenDevoluciones> GetResumenDevolucionesAsync(DateTime desde, DateTime hasta)
    {
        var devoluciones = await _db.Ventas
            .Include(v => v.Detalles)
            .Where(v => v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado == EstadoVenta.Anulada)
            .ToListAsync();

        return new ResumenDevoluciones
        {
            TotalDevoluciones = devoluciones.Count,
            MontoTotalDevuelto = devoluciones.Sum(v => v.Total),
            UnidadesRestauradas = devoluciones.SelectMany(v => v.Detalles).Sum(d => d.Cantidad)
        };
    }

    public async Task<List<ProductoVendido>> GetProductosMasVendidosAsync(DateTime desde, DateTime hasta)
    {
        return await _db.VentaDetalles
            .Include(d => d.Producto)
            .Include(d => d.Venta)
            .Where(d => d.Venta.Fecha >= desde && d.Venta.Fecha <= hasta.AddDays(1) && d.Venta.Estado != EstadoVenta.Anulada)
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
            .Where(d => d.Venta.Fecha >= desde && d.Venta.Fecha <= hasta.AddDays(1) && d.Venta.Estado != EstadoVenta.Anulada)
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
    public int DevolucionesDelDia { get; set; }
    public decimal TotalDevolucionesDelDia { get; set; }
    public decimal TotalFiados { get; set; }
    public int ProductosStockBajo { get; set; }
    public int ProductosPorVencer { get; set; }
    public int ProductosVencidos { get; set; }
}

public class ResumenDevoluciones
{
    public int TotalDevoluciones { get; set; }
    public decimal MontoTotalDevuelto { get; set; }
    public decimal UnidadesRestauradas { get; set; }
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
