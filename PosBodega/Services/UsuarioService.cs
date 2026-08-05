using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class UsuarioService
{
    private readonly AppDbContext _db;

    public UsuarioService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Usuario>> GetAllAsync()
    {
        return await _db.Usuarios
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _db.Usuarios.FindAsync(id);
    }

    public async Task<string?> CreateAsync(string nombre, string nombreUsuario, string password, Rol rol)
    {
        // Check if username already exists
        var exists = await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
        if (exists)
            return "El nombre de usuario ya existe.";

        var usuario = new Usuario
        {
            Nombre = nombre,
            NombreUsuario = nombreUsuario,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Rol = rol,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> UpdateAsync(int id, string nombre, Rol rol, bool activo)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null)
            return "Usuario no encontrado.";

        usuario.Nombre = nombre;
        usuario.Rol = rol;
        usuario.Activo = activo;

        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> ChangePasswordAsync(int id, string newPassword)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null)
            return "Usuario no encontrado.";

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();
        return null;
    }
}
