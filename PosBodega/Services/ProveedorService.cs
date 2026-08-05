using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class ProveedorService
{
    private readonly AppDbContext _db;

    public ProveedorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Proveedor>> GetAllAsync()
    {
        return await _db.Proveedores
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Proveedor?> GetByIdAsync(int id)
    {
        return await _db.Proveedores.FindAsync(id);
    }

    public async Task CreateAsync(Proveedor proveedor)
    {
        _db.Proveedores.Add(proveedor);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Proveedor proveedor)
    {
        _db.Proveedores.Update(proveedor);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);
        if (proveedor != null)
        {
            proveedor.Activo = false;
            await _db.SaveChangesAsync();
        }
    }
}
