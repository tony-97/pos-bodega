using Microsoft.EntityFrameworkCore;
using PosBodega.Models;

namespace PosBodega.Data;

public class AppDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<CompraDetalle> CompraDetalles => Set<CompraDetalle>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();
    public DbSet<PagoFiado> PagosFiado => Set<PagoFiado>();

    private readonly string _dbPath;

    public AppDbContext()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "posbodega.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Producto -> Categoria relationship
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Compra -> Proveedor
        modelBuilder.Entity<Compra>()
            .HasOne(c => c.Proveedor)
            .WithMany()
            .HasForeignKey(c => c.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Compra -> Usuario
        modelBuilder.Entity<Compra>()
            .HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // CompraDetalle -> Compra
        modelBuilder.Entity<CompraDetalle>()
            .HasOne(d => d.Compra)
            .WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        // CompraDetalle -> Producto
        modelBuilder.Entity<CompraDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Venta -> Cliente (optional)
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Venta -> Usuario
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // VentaDetalle -> Venta
        modelBuilder.Entity<VentaDetalle>()
            .HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        // VentaDetalle -> Producto
        modelBuilder.Entity<VentaDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // PagoFiado -> Cliente
        modelBuilder.Entity<PagoFiado>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pagos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on NombreUsuario
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

        // Unique constraint on CodigoBarras (when not null)
        modelBuilder.Entity<Producto>()
            .HasIndex(p => p.CodigoBarras)
            .IsUnique()
            .HasFilter("[CodigoBarras] IS NOT NULL");

        // --- Seed Data ---

        // Default admin user (password: admin123)
        modelBuilder.Entity<Usuario>().HasData(new Usuario
        {
            Id = 1,
            Nombre = "Administrador",
            NombreUsuario = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Rol = Rol.Administrador,
            Activo = true,
            FechaCreacion = new DateTime(2025, 1, 1)
        });

        // Default categories
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Abarrotes", Descripcion = "Productos básicos de despensa" },
            new Categoria { Id = 2, Nombre = "Bebidas", Descripcion = "Gaseosas, jugos, agua y bebidas en general" },
            new Categoria { Id = 3, Nombre = "Limpieza", Descripcion = "Productos de limpieza del hogar" },
            new Categoria { Id = 4, Nombre = "Lácteos", Descripcion = "Leche, yogurt, queso y derivados" },
            new Categoria { Id = 5, Nombre = "Golosinas", Descripcion = "Dulces, galletas y snacks" },
            new Categoria { Id = 6, Nombre = "Otros", Descripcion = "Productos varios" }
        );
    }
}
