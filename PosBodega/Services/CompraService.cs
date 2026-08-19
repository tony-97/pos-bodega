using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class CompraService
{
    private readonly AppDbContext _db;

    public CompraService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Compra>> GetAllAsync()
    {
        return await _db.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.Usuario)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
    }

    public async Task<Compra?> GetByIdAsync(int id)
    {
        return await _db.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.Usuario)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Registers a purchase and atomically increments stock for each product.
    /// </summary>
    public async Task RegistrarCompraAsync(Compra compra, List<CompraDetalle> detalles)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Calculate subtotals and total
            decimal total = 0;
            foreach (var detalle in detalles)
            {
                detalle.Subtotal = detalle.Cantidad * detalle.CostoUnitario;
                total += detalle.Subtotal;
            }

            compra.Total = total;
            compra.Fecha = DateTime.Now;
            compra.Detalles = detalles;

            _db.Compras.Add(compra);

            // Increment stock for each product and update expiration date if provided
            foreach (var detalle in detalles)
            {
                var producto = await _db.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.StockActual += detalle.Cantidad;
                    // Update cost price if different
                    producto.Costo = detalle.CostoUnitario;
                    // Update expiration date if specified in purchase detail
                    if (detalle.FechaVencimiento.HasValue)
                    {
                        producto.FechaVencimiento = detalle.FechaVencimiento.Value;
                    }
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
