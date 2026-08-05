using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class CategoriaService
{
    private readonly AppDbContext _db;

    public CategoriaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Categoria>> GetAllAsync()
    {
        return await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<Categoria?> GetByIdAsync(int id)
    {
        return await _db.Categorias.FindAsync(id);
    }

    public async Task CreateAsync(Categoria categoria)
    {
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        _db.Categorias.Update(categoria);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria != null)
        {
            _db.Categorias.Remove(categoria);
            await _db.SaveChangesAsync();
        }
    }
}
