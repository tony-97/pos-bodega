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
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();

        var trimmed = query.Trim();
        var lower = trimmed.ToLower();
        var isNumeric = int.TryParse(trimmed.TrimStart('#'), out int parsedId);

        return await _db.Clientes
            .Where(c => c.Activo &&
                ((isNumeric && c.Id == parsedId) ||
                 c.Nombre.ToLower().Contains(lower) ||
                 (c.Dni != null && c.Dni.Contains(trimmed)) ||
                 (c.Ruc != null && c.Ruc.Contains(trimmed)) ||
                 (c.Telefono != null && c.Telefono.Contains(trimmed)) ||
                 (c.Direccion != null && c.Direccion.ToLower().Contains(lower))))
            .OrderBy(c => c.Nombre)
            .Take(30)
            .ToListAsync();
    }

    public async Task<Cliente?> GetByDniOrRucAsync(string doc)
    {
        if (string.IsNullOrWhiteSpace(doc)) return null;
        var trimmed = doc.Trim();
        return await _db.Clientes
            .FirstOrDefaultAsync(c => c.Activo && (c.Dni == trimmed || c.Ruc == trimmed));
    }

    public async Task<Cliente?> GetDuplicateDniAsync(string dni, int excludeId = 0)
    {
        if (string.IsNullOrWhiteSpace(dni)) return null;
        var trimmed = dni.Trim();
        return await _db.Clientes
            .FirstOrDefaultAsync(c => c.Activo && c.Id != excludeId && c.Dni == trimmed);
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

