using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class ClienteService
{
    private readonly AppDbContext _db;

    public ClienteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Cliente>> GetAllAsync()
    {
        return await _db.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<List<Cliente>> GetDeudoresAsync()
    {
        return await _db.Clientes
            .Where(c => c.Activo && c.SaldoDeudor > 0)
            .OrderByDescending(c => c.SaldoDeudor)
            .ToListAsync();
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        return await _db.Clientes
            .Include(c => c.Pagos.OrderByDescending(p => p.Fecha))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Cliente>> SearchAsync(string query)
    {
        var lower = query.ToLower();
        return await _db.Clientes
            .Where(c => c.Activo &&
                (c.Nombre.ToLower().Contains(lower) ||
                 (c.Dni != null && c.Dni.Contains(query))))
            .OrderBy(c => c.Nombre)
            .Take(20)
            .ToListAsync();
    }

    public async Task CreateAsync(Cliente cliente)
    {
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        _db.Clientes.Update(cliente);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente != null)
        {
            cliente.Activo = false;
            await _db.SaveChangesAsync();
        }
    }
}
