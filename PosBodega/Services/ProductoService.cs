using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class ProductoService
{
    private readonly AppDbContext _db;

    public ProductoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Producto>> GetAllAsync()
    {
        return await _db.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<List<Producto>> GetByCategoriaAsync(int categoriaId)
    {
        return await _db.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.CategoriaId == categoriaId)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Producto?> GetByIdAsync(int id)
    {
        return await _db.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Producto?> GetByBarcodeAsync(string barcode)
    {
        return await _db.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.CodigoBarras == barcode && p.Activo);
    }

    public async Task<List<Producto>> SearchAsync(string query)
    {
        var lower = query.ToLower();
        return await _db.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo &&
                (p.Nombre.ToLower().Contains(lower) ||
                 (p.CodigoBarras != null && p.CodigoBarras.Contains(query))))
            .OrderBy(p => p.Nombre)
            .Take(20)
            .ToListAsync();
    }

    public async Task<List<Producto>> GetLowStockAsync()
    {
        return await _db.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
            .OrderBy(p => p.StockActual)
            .ToListAsync();
    }

    /// <summary>
    /// Gets perishable products that are either expired or expiring within the specified number of days (default 15 days), with stock > 0.
    /// </summary>
    public async Task<List<Producto>> GetExpiringProductsAsync(int daysThreshold = 15)
    {
        var limitDate = DateTime.Today.AddDays(daysThreshold);
        return await _db.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.FechaVencimiento != null && p.FechaVencimiento <= limitDate && p.StockActual > 0)
            .OrderBy(p => p.FechaVencimiento)
            .ToListAsync();
    }

    /// <summary>
    /// Gets products that have already expired and still have stock > 0.
    /// </summary>
    public async Task<List<Producto>> GetExpiredProductsAsync()
    {
        var today = DateTime.Today;
        return await _db.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.FechaVencimiento != null && p.FechaVencimiento < today && p.StockActual > 0)
            .OrderBy(p => p.FechaVencimiento)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all active products that have an expiration date configured.
    /// </summary>
    public async Task<List<Producto>> GetPerishableProductsAsync()
    {
        return await _db.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.FechaVencimiento != null)
            .OrderBy(p => p.FechaVencimiento)
            .ToListAsync();
    }

    public async Task CreateAsync(Producto producto)
    {
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Producto producto)
    {
        _db.Productos.Update(producto);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto != null)
        {
            producto.Activo = false;
            await _db.SaveChangesAsync();
        }
    }
}
