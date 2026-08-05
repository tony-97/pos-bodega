using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class VentaService
{
    private readonly AppDbContext _db;

    public VentaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Venta>> GetAllAsync()
    {
        return await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<List<Venta>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.Fecha >= from && v.Fecha <= to)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<Venta?> GetByIdAsync(int id)
    {
        return await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    /// <summary>
    /// Registers a sale, atomically decrements stock, and optionally updates client debt.
    /// Returns error message if validation fails, null on success.
    /// </summary>
    public async Task<string?> RegistrarVentaAsync(Venta venta, List<VentaDetalle> detalles)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Validate stock availability
            foreach (var detalle in detalles)
            {
                var producto = await _db.Productos.FindAsync(detalle.ProductoId);
                if (producto == null)
                    return $"Producto con ID {detalle.ProductoId} no encontrado.";

                if (producto.StockActual < detalle.Cantidad)
                    return $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.StockActual}, Solicitado: {detalle.Cantidad}";
            }

            // Calculate subtotals and total
            decimal total = 0;
            foreach (var detalle in detalles)
            {
                detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                total += detalle.Subtotal;
            }

            venta.Total = total;
            venta.Fecha = DateTime.Now;
            venta.Detalles = detalles;

            _db.Ventas.Add(venta);

            // Decrement stock for each product
            foreach (var detalle in detalles)
            {
                var producto = await _db.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.StockActual -= detalle.Cantidad;
                }
            }

            // If fiado, update client debt
            if (venta.EsFiado && venta.ClienteId.HasValue)
            {
                var cliente = await _db.Clientes.FindAsync(venta.ClienteId.Value);
                if (cliente != null)
                {
                    cliente.SaldoDeudor += total;
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return null; // Success
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return $"Error al registrar la venta: {ex.Message}";
        }
    }
}
