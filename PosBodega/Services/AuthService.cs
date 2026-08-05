using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private Usuario? _currentUser;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public Usuario? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.Rol == Rol.Administrador;
    public bool IsVendedor => _currentUser?.Rol == Rol.Vendedor;

    public event Action? OnAuthStateChanged;

    public async Task<bool> LoginAsync(string username, string password)
    {
        var user = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == username && u.Activo);

        if (user == null)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return false;

        _currentUser = user;
        OnAuthStateChanged?.Invoke();
        return true;
    }

    public void Logout()
    {
        _currentUser = null;
        OnAuthStateChanged?.Invoke();
    }
}
