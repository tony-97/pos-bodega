using Microsoft.EntityFrameworkCore;
using PosBodega.Models;

namespace PosBodega.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();

        // Ensure new columns exist on existing databases (safe migration)
        try
        {
            db.Database.ExecuteSqlRaw("ALTER TABLE Productos ADD COLUMN FechaVencimiento TEXT;");
        }
        catch { /* Column already exists */ }

        try
        {
            db.Database.ExecuteSqlRaw("ALTER TABLE CompraDetalles ADD COLUMN FechaVencimiento TEXT;");
        }
        catch { /* Column already exists */ }

        // Ventas new columns
        var ventaColumns = new[]
        {
            "ALTER TABLE Ventas ADD COLUMN Serie TEXT;",
            "ALTER TABLE Ventas ADD COLUMN Numero INTEGER DEFAULT 0;",
            "ALTER TABLE Ventas ADD COLUMN NumeroComprobante TEXT;",
            "ALTER TABLE Ventas ADD COLUMN Estado INTEGER DEFAULT 0;",
            "ALTER TABLE Ventas ADD COLUMN ClienteNombre TEXT;",
            "ALTER TABLE Ventas ADD COLUMN ClienteTipoDocumento INTEGER DEFAULT 0;",
            "ALTER TABLE Ventas ADD COLUMN ClienteDocumento TEXT;",
            "ALTER TABLE Ventas ADD COLUMN ClienteDireccion TEXT;",
            "ALTER TABLE Ventas ADD COLUMN Subtotal TEXT DEFAULT '0';",
            "ALTER TABLE Ventas ADD COLUMN Igv TEXT DEFAULT '0';",
            "ALTER TABLE Ventas ADD COLUMN MontoRecibido TEXT;",
            "ALTER TABLE Ventas ADD COLUMN Vuelto TEXT;",
            "ALTER TABLE Ventas ADD COLUMN TicketOrigenId INTEGER;",
            "ALTER TABLE Ventas ADD COLUMN ComprobanteDestinoId INTEGER;",
            "ALTER TABLE Ventas ADD COLUMN MotivoAnulacion TEXT;"
        };

        foreach (var sql in ventaColumns)
        {
            try { db.Database.ExecuteSqlRaw(sql); } catch { /* Ignore if already exists */ }
        }

        // Clientes new columns
        try { db.Database.ExecuteSqlRaw("ALTER TABLE Clientes ADD COLUMN Ruc TEXT;"); } catch { }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE Clientes ADD COLUMN Direccion TEXT;"); } catch { }

        // 1. Ensure Vendedor user exists
        if (!db.Usuarios.Any(u => u.NombreUsuario == "vendedor"))
        {
            db.Usuarios.Add(new Usuario
            {
                Nombre = "Vendedor de Turno",
                NombreUsuario = "vendedor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("vendedor123"),
                Rol = Rol.Vendedor,
                Activo = true,
                FechaCreacion = DateTime.Now
            });
            db.SaveChanges();
        }

        // 2. Ensure Categories exist
        if (!db.Categorias.Any())
        {
            db.Categorias.AddRange(
                new Categoria { Nombre = "Abarrotes", Descripcion = "Productos básicos de despensa" },
                new Categoria { Nombre = "Bebidas", Descripcion = "Gaseosas, jugos, agua y bebidas en general" },
                new Categoria { Nombre = "Limpieza", Descripcion = "Productos de limpieza del hogar" },
                new Categoria { Nombre = "Lácteos", Descripcion = "Leche, yogurt, queso y derivados" },
                new Categoria { Nombre = "Golosinas", Descripcion = "Dulces, galletas y snacks" },
                new Categoria { Nombre = "Otros", Descripcion = "Productos varios" }
            );
            db.SaveChanges();
        }

        // 3. Ensure Suppliers exist
        if (!db.Proveedores.Any())
        {
            db.Proveedores.AddRange(
                new Proveedor { Nombre = "Distribuidora Huánuco S.A.C.", Contacto = "Juan Pérez", Telefono = "962123456", Activo = true },
                new Proveedor { Nombre = "Comercial Central E.I.R.L.", Contacto = "María Gómez", Telefono = "951987654", Activo = true },
                new Proveedor { Nombre = "Alicorp Perú", Contacto = "Ventas Directas", Telefono = "080012345", Activo = true }
            );
            db.SaveChanges();
        }

        // 4. Ensure Products exist
        if (!db.Productos.Any())
        {
            var catAbarrotes = db.Categorias.FirstOrDefault(c => c.Nombre == "Abarrotes")?.Id ?? 1;
            var catBebidas = db.Categorias.FirstOrDefault(c => c.Nombre == "Bebidas")?.Id ?? 2;
            var catLimpieza = db.Categorias.FirstOrDefault(c => c.Nombre == "Limpieza")?.Id ?? 3;
            var catLacteos = db.Categorias.FirstOrDefault(c => c.Nombre == "Lácteos")?.Id ?? 4;
            var catGolosinas = db.Categorias.FirstOrDefault(c => c.Nombre == "Golosinas")?.Id ?? 5;

            db.Productos.AddRange(
                new Producto { Nombre = "Arroz Costeño Superior 1kg", CategoriaId = catAbarrotes, UnidadMedida = UnidadMedida.Kilogramo, CodigoBarras = "7750123456789", Costo = 4.20m, PrecioVenta = 4.80m, StockActual = 50, StockMinimo = 10, FechaVencimiento = DateTime.Today.AddMonths(8), Activo = true },
                new Producto { Nombre = "Aceite Primor Clásico 1L", CategoriaId = catAbarrotes, UnidadMedida = UnidadMedida.Litro, CodigoBarras = "7750123456790", Costo = 7.50m, PrecioVenta = 9.00m, StockActual = 30, StockMinimo = 8, FechaVencimiento = DateTime.Today.AddMonths(6), Activo = true },
                new Producto { Nombre = "Azúcar Rubia Dulfina 1kg", CategoriaId = catAbarrotes, UnidadMedida = UnidadMedida.Kilogramo, CodigoBarras = "7750123456791", Costo = 3.60m, PrecioVenta = 4.20m, StockActual = 40, StockMinimo = 10, FechaVencimiento = null, Activo = true },
                new Producto { Nombre = "Inca Kola 500ml NR", CategoriaId = catBebidas, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456792", Costo = 2.20m, PrecioVenta = 3.00m, StockActual = 48, StockMinimo = 12, FechaVencimiento = DateTime.Today.AddMonths(3), Activo = true },
                new Producto { Nombre = "Coca Cola 500ml NR", CategoriaId = catBebidas, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456793", Costo = 2.20m, PrecioVenta = 3.00m, StockActual = 48, StockMinimo = 12, FechaVencimiento = DateTime.Today.AddMonths(4), Activo = true },
                new Producto { Nombre = "Agua San Luis 625ml s/gas", CategoriaId = catBebidas, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456794", Costo = 1.20m, PrecioVenta = 2.00m, StockActual = 60, StockMinimo = 15, FechaVencimiento = DateTime.Today.AddYears(1), Activo = true },
                new Producto { Nombre = "Detergente Ace 800g", CategoriaId = catLimpieza, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456795", Costo = 6.80m, PrecioVenta = 8.50m, StockActual = 20, StockMinimo = 5, FechaVencimiento = null, Activo = true },
                new Producto { Nombre = "Jabón Bolívar Blanco", CategoriaId = catLimpieza, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456796", Costo = 2.80m, PrecioVenta = 3.50m, StockActual = 25, StockMinimo = 6, FechaVencimiento = null, Activo = true },
                new Producto { Nombre = "Leche Gloria Azul 400g", CategoriaId = catLacteos, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456797", Costo = 3.60m, PrecioVenta = 4.30m, StockActual = 72, StockMinimo = 24, FechaVencimiento = DateTime.Today.AddDays(25), Activo = true },
                new Producto { Nombre = "Yogurt La Live Fresa 1L", CategoriaId = catLacteos, UnidadMedida = UnidadMedida.Litro, CodigoBarras = "7750123456798", Costo = 5.50m, PrecioVenta = 7.00m, StockActual = 15, StockMinimo = 5, FechaVencimiento = DateTime.Today.AddDays(6), Activo = true },
                new Producto { Nombre = "Galletas Chomp Naranja 40g", CategoriaId = catGolosinas, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456799", Costo = 1.20m, PrecioVenta = 1.80m, StockActual = 30, StockMinimo = 10, FechaVencimiento = DateTime.Today.AddDays(45), Activo = true },
                new Producto { Nombre = "Chocolate Sublime Clásico 30g", CategoriaId = catGolosinas, UnidadMedida = UnidadMedida.Unidad, CodigoBarras = "7750123456800", Costo = 1.50m, PrecioVenta = 2.20m, StockActual = 40, StockMinimo = 10, FechaVencimiento = DateTime.Today.AddMonths(3), Activo = true }
            );
            db.SaveChanges();
        }
        else
        {
            // Populate/update expiration dates on existing database items if they are currently null
            var defaultExpirations = new Dictionary<string, DateTime>
            {
                { "Aceite Primor", DateTime.Today.AddMonths(6) },
                { "Chocolate Sublime", DateTime.Today.AddMonths(3) },
                { "Coca Cola", DateTime.Today.AddMonths(4) },
                { "Galletas Chomp", DateTime.Today.AddDays(45) },
                { "Inca Kola", DateTime.Today.AddMonths(3) },
                { "Yogurt", DateTime.Today.AddDays(6) },
                { "Leche", DateTime.Today.AddDays(25) },
                { "Arroz Costeño", DateTime.Today.AddMonths(8) },
                { "Agua San Luis", DateTime.Today.AddYears(1) }
            };

            bool changed = false;
            foreach (var item in defaultExpirations)
            {
                var prod = db.Productos.FirstOrDefault(p => p.Nombre.Contains(item.Key));
                if (prod != null && prod.FechaVencimiento == null)
                {
                    prod.FechaVencimiento = item.Value;
                    changed = true;
                }
            }

            if (changed)
            {
                db.SaveChanges();
            }
        }

        // 5. Ensure Clients exist
        if (!db.Clientes.Any())
        {
            db.Clientes.AddRange(
                new Cliente { Nombre = "Carlos Mendoza (Vecino)", Dni = "45678912", Telefono = "962000111", SaldoDeudor = 35.50m, LimiteCredito = 150.00m, Activo = true },
                new Cliente { Nombre = "Ana Torres", Dni = "71234567", Telefono = "951000222", SaldoDeudor = 0.00m, LimiteCredito = 100.00m, Activo = true }
            );
            db.SaveChanges();
        }

        // 6. Backfill any existing sales with empty NumeroComprobante
        var legacySales = db.Ventas.Where(v => string.IsNullOrEmpty(v.NumeroComprobante)).ToList();
        if (legacySales.Any())
        {
            int boletaSeq = 1;
            int ticketSeq = 1;
            foreach (var v in legacySales.OrderBy(v => v.Fecha))
            {
                if (v.TipoComprobante == TipoComprobante.Boleta)
                {
                    v.Serie = "B001";
                    v.Numero = boletaSeq++;
                    v.NumeroComprobante = $"{v.Serie}-{v.Numero:D6}";
                }
                else if (v.TipoComprobante == TipoComprobante.Factura)
                {
                    v.Serie = "F001";
                    v.Numero = 1;
                    v.NumeroComprobante = $"{v.Serie}-{v.Numero:D6}";
                }
                else
                {
                    v.Serie = "T001";
                    v.Numero = ticketSeq++;
                    v.NumeroComprobante = $"{v.Serie}-{v.Numero:D6}";
                }

                if (string.IsNullOrEmpty(v.ClienteNombre))
                {
                    v.ClienteNombre = "Clientes Varios";
                }

                v.Subtotal = Math.Round(v.Total / 1.18m, 2);
                v.Igv = v.Total - v.Subtotal;
            }
            db.SaveChanges();
        }
    }
}
