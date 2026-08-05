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

        // Default users (admin123 and vendedor123)
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nombre = "Administrador",
                NombreUsuario = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Rol = Rol.Administrador,
                Activo = true,
                FechaCreacion = new DateTime(2025, 1, 1)
            },
            new Usuario
            {
                Id = 2,
                Nombre = "Vendedor de Turno",
                NombreUsuario = "vendedor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("vendedor123"),
                Rol = Rol.Vendedor,
                Activo = true,
                FechaCreacion = new DateTime(2025, 1, 1)
            }
        );

        // Default categories
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Abarrotes", Descripcion = "Productos básicos de despensa" },
            new Categoria { Id = 2, Nombre = "Bebidas", Descripcion = "Gaseosas, jugos, agua y bebidas en general" },
            new Categoria { Id = 3, Nombre = "Limpieza", Descripcion = "Productos de limpieza del hogar" },
            new Categoria { Id = 4, Nombre = "Lácteos", Descripcion = "Leche, yogurt, queso y derivados" },
            new Categoria { Id = 5, Nombre = "Golosinas", Descripcion = "Dulces, galletas y snacks" },
            new Categoria { Id = 6, Nombre = "Otros", Descripcion = "Productos varios" }
        );

        // Sample suppliers
        modelBuilder.Entity<Proveedor>().HasData(
            new Proveedor { Id = 1, Nombre = "Distribuidora Huánuco S.A.C.", Contacto = "Juan Pérez", Telefono = "962123456", Activo = true },
            new Proveedor { Id = 2, Nombre = "Comercial Central E.I.R.L.", Contacto = "María Gómez", Telefono = "951987654", Activo = true },
            new Proveedor { Id = 3, Nombre = "Alicorp Perú", Contacto = "Ventas Directas", Telefono = "080012345", Activo = true }
        );

        // Sample products
        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Arroz Costeño Superior 1kg", CategoriaId = 1, UnidadMedida = UnidadMedida.Kilogramo, CodigoBarras = "7750123456789", Costo = 4.20m, PrecioVenta = 4.80m, StockActual = 50, StockMinimo = 10, Activo = true },
            new Producto { Id = 2, Nombre = "Aceite Primor Clásico 1L", CategoriaId = 1, UnidadMedida = UnidadMedida.Litro, CodigoBarras = "7750123456790", Costo = 7.50m, PrecioVenta = 9.00m, StockActual = 30, StockMinimo = 8, Activo = true },
            new Producto { Id = 3, Nombre = "Azúcar Rubia Dulfina 1kg", CategoriaId = 1, UnidadMedida = UnidadMedida.Kilogramo, CodigoBarras = "7750123456791", Costo = 3.60m, PrecioVenta = 4.20m, StockActual = 40, StockMinimo = 10, Activo = true },
            new Producto { Id = 4, Nombre = "Inca Kola 500ml NR", CategoriaId = 2, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456792", Costo = 2.20m, PrecioVenta = 3.00m, StockActual = 48, StockMinimo = 12, Activo = true },
            new Producto { Id = 5, Nombre = "Coca Cola 500ml NR", CategoriaId = 2, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456793", Costo = 2.20m, PrecioVenta = 3.00m, StockActual = 48, StockMinimo = 12, Activo = true },
            new Producto { Id = 6, Nombre = "Agua San Luis 625ml s/gas", CategoriaId = 2, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456794", Costo = 1.20m, PrecioVenta = 2.00m, StockActual = 60, StockMinimo = 15, Activo = true },
            new Producto { Id = 7, Nombre = "Detergente Ace 800g", CategoriaId = 3, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456795", Costo = 6.80m, PrecioVenta = 8.50m, StockActual = 20, StockMinimo = 5, Activo = true },
            new Producto { Id = 8, Nombre = "Jabón Bolívar Blanco", CategoriaId = 3, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456796", Costo = 2.80m, PrecioVenta = 3.50m, StockActual = 25, StockMinimo = 6, Activo = true },
            new Producto { Id = 9, Nombre = "Leche Gloria Azul 400g", CategoriaId = 4, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456797", Costo = 3.60m, PrecioVenta = 4.30m, StockActual = 72, StockMinimo = 24, Activo = true },
            new Producto { Id = 10, Nombre = "Yogurt La Live Fresa 1L", CategoriaId = 4, UnidadMedida = UnidadMedida.Litro, CodigoBarras = "7750123456798", Costo = 5.50m, PrecioVenta = 7.00m, StockActual = 15, StockMinimo = 5, Activo = true },
            new Producto { Id = 11, Nombre = "Galletas Chomp Naranja 40g", CategoriaId = 5, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456799", Costo = 1.20m, PrecioVenta = 1.80m, StockActual = 30, StockMinimo = 10, Activo = true },
            new Producto { Id = 12, Nombre = "Chocolate Sublime Clásico 30g", CategoriaId = 5, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456800", Costo = 1.50m, PrecioVenta = 2.20m, StockActual = 40, StockMinimo = 10, Activo = true }
        );

        // Sample clients
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nombre = "Carlos Mendoza (Vecino)", Dni = "45678912", Telefono = "962000111", SaldoDeudor = 35.50m, LimiteCredito = 150.00m, Activo = true },
            new Cliente { Id = 2, Nombre = "Ana Torres", Dni = "71234567", Telefono = "951000222", SaldoDeudor = 0.00m, LimiteCredito = 100.00m, Activo = true }
        );
    }
}
