using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class VentaService
{
    private readonly AppDbContext _db;
    private readonly EmpresaService _empresaService;

    public VentaService(AppDbContext db, EmpresaService empresaService)
    {
        _db = db;
        _empresaService = empresaService;
    }

    public async Task<List<Venta>> GetAllAsync()
    {
        return await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<List<Venta>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.Fecha >= from && v.Fecha <= to)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<Venta?> GetByIdAsync(int id)
    {
        return await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    /// <summary>
    /// Gets the next correlative series and number for a given document type (Boleta or Ticket under Nuevo RUS).
    /// </summary>
    public async Task<(string Serie, int Numero, string NumeroComprobante)> GetNextCorrelativoAsync(TipoComprobante tipo)
    {
        var config = _empresaService.GetConfiguracion();
        string serie = tipo == TipoComprobante.Boleta
            ? config.SerieBoleta
            : config.SerieTicket;

        var lastNumber = await _db.Ventas
            .Where(v => v.Serie == serie)
            .MaxAsync(v => (int?)v.Numero) ?? 0;

        int nextNum = lastNumber + 1;
        string numeroComprobante = $"{serie}-{nextNum:D6}";
        return (serie, nextNum, numeroComprobante);
    }

    /// <summary>
    /// Registers a sale, calculates SUNAT tax breakdown, atomically decrements stock, and assigns independent sequential correlative.
    /// Under Nuevo RUS, emissions are Boletas de Venta (B001) or internal control Tickets (T001).
    /// </summary>
    public async Task<(Venta? Venta, string? Error)> RegistrarVentaAsync(Venta venta, List<VentaDetalle> detalles)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            if (detalles == null || detalles.Count == 0)
                return (null, "No hay productos en el comprobante.");

            // Calculate subtotals and total
            decimal total = 0;
            foreach (var detalle in detalles)
            {
                var producto = await _db.Productos.FindAsync(detalle.ProductoId);
                if (producto == null)
                    return (null, $"Producto con ID {detalle.ProductoId} no encontrado.");

                if (producto.StockActual < detalle.Cantidad)
                    return (null, $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.StockActual}, Solicitado: {detalle.Cantidad}");

                detalle.Subtotal = Math.Round(detalle.Cantidad * detalle.PrecioUnitario, 2);
                total += detalle.Subtotal;
            }

            // SUNAT validation (Nuevo RUS rules)
            if (!SunatValidator.ValidarComprobante(
                venta.TipoComprobante,
                total,
                venta.ClienteNombre,
                venta.ClienteDocumento,
                out var validationError))
            {
                return (null, validationError);
            }

            // Set document values & independent correlative
            var correlativo = await GetNextCorrelativoAsync(venta.TipoComprobante);
            venta.Serie = correlativo.Serie;
            venta.Numero = correlativo.Numero;
            venta.NumeroComprobante = correlativo.NumeroComprobante;

            venta.Total = total;
            venta.Subtotal = Math.Round(total / 1.18m, 2);
            venta.Igv = total - venta.Subtotal;
            venta.Fecha = DateTime.Now;
            venta.Estado = EstadoVenta.Emitida;
            venta.Detalles = detalles;

            // Set customer defaults if empty
            if (string.IsNullOrWhiteSpace(venta.ClienteNombre))
            {
                venta.ClienteNombre = "Clientes Varios";
            }

            if (!string.IsNullOrWhiteSpace(venta.ClienteDocumento))
            {
                var cleanDoc = venta.ClienteDocumento.Trim();
                if (cleanDoc.Length == 8)
                    venta.ClienteTipoDocumento = TipoDocumentoIdentidad.DNI;
                else if (cleanDoc.Length == 11)
                    venta.ClienteTipoDocumento = TipoDocumentoIdentidad.RUC;
                else
                    venta.ClienteTipoDocumento = TipoDocumentoIdentidad.SinDocumento;
            }
            else
            {
                venta.ClienteTipoDocumento = TipoDocumentoIdentidad.SinDocumento;
            }

            _db.Ventas.Add(venta);

            // Decrement stock for each product
            foreach (var detalle in detalles)
            {
                var producto = await _db.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.StockActual -= detalle.Cantidad;
                }
            }

            // If fiado, update client debt
            if (venta.EsFiado && venta.ClienteId.HasValue)
            {
                var cliente = await _db.Clientes.FindAsync(venta.ClienteId.Value);
                if (cliente != null)
                {
                    cliente.SaldoDeudor += total;
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            // Load navigation properties for receipt display
            await _db.Entry(venta).Reference(v => v.Usuario).LoadAsync();
            if (venta.ClienteId.HasValue)
            {
                await _db.Entry(venta).Reference(v => v.Cliente).LoadAsync();
            }

            return (venta, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (null, $"Error al registrar la venta: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts an existing internal ticket into an official Boleta de Venta (B001) under Nuevo RUS.
    /// Does not decrement stock a second time because stock was already reserved upon ticket creation.
    /// </summary>
    public async Task<(Venta? Comprobante, string? Error)> ConvertirTicketAComprobanteAsync(
        int ticketId,
        TipoComprobante nuevoTipo,
        string clienteNombre,
        string? clienteDocumento,
        string? clienteDireccion = null,
        MetodoPago? nuevoMetodoPago = null)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var ticket = await _db.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.Id == ticketId);

            if (ticket == null)
                return (null, "Ticket no encontrado.");

            if (ticket.TipoComprobante != TipoComprobante.TicketInterno)
                return (null, "Solo los Tickets de Venta Internos pueden ser convertidos a Boleta de Venta.");

            if (ticket.Estado == EstadoVenta.Anulada)
                return (null, "No se puede convertir un ticket anulado.");

            if (ticket.ComprobanteDestinoId.HasValue)
                return (null, "Este ticket ya ha sido convertido previamente a un comprobante oficial.");

            // Under Nuevo RUS, conversion target is Boleta de Venta
            var tipoDestino = TipoComprobante.Boleta;

            // Validate SUNAT compliance for the new Boleta
            if (!SunatValidator.ValidarComprobante(
                tipoDestino,
                ticket.Total,
                clienteNombre,
                clienteDocumento,
                out var validationError))
            {
                return (null, validationError);
            }

            var correlativo = await GetNextCorrelativoAsync(tipoDestino);

            var nuevaVenta = new Venta
            {
                Fecha = DateTime.Now,
                Serie = correlativo.Serie,
                Numero = correlativo.Numero,
                NumeroComprobante = correlativo.NumeroComprobante,
                TipoComprobante = tipoDestino,
                Estado = EstadoVenta.Emitida,
                ClienteId = ticket.ClienteId,
                ClienteNombre = string.IsNullOrWhiteSpace(clienteNombre) ? "Clientes Varios" : clienteNombre.Trim(),
                ClienteDocumento = clienteDocumento?.Trim(),
                ClienteDireccion = clienteDireccion?.Trim(),
                UsuarioId = ticket.UsuarioId,
                MetodoPago = nuevoMetodoPago ?? ticket.MetodoPago,
                Subtotal = ticket.Subtotal,
                Igv = ticket.Igv,
                Total = ticket.Total,
                MontoRecibido = ticket.MontoRecibido,
                Vuelto = ticket.Vuelto,
                EsFiado = ticket.EsFiado,
                Observaciones = $"Emitido a partir del Ticket {ticket.NumeroComprobante}",
                TicketOrigenId = ticket.Id
            };

            if (!string.IsNullOrWhiteSpace(nuevaVenta.ClienteDocumento))
            {
                nuevaVenta.ClienteTipoDocumento = nuevaVenta.ClienteDocumento.Length == 8
                    ? TipoDocumentoIdentidad.DNI
                    : TipoDocumentoIdentidad.RUC;
            }
            else
            {
                nuevaVenta.ClienteTipoDocumento = TipoDocumentoIdentidad.SinDocumento;
            }

            // Clone details (without deducting stock again!)
            nuevaVenta.Detalles = ticket.Detalles.Select(d => new VentaDetalle
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList();

            _db.Ventas.Add(nuevaVenta);
            await _db.SaveChangesAsync();

            // Link ticket to destination document
            ticket.ComprobanteDestinoId = nuevaVenta.Id;
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            // Load relationships for receipt view
            await _db.Entry(nuevaVenta).Reference(v => v.Usuario).LoadAsync();
            if (nuevaVenta.ClienteId.HasValue)
            {
                await _db.Entry(nuevaVenta).Reference(v => v.Cliente).LoadAsync();
            }

            return (nuevaVenta, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (null, $"Error al convertir ticket: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancels (anulates) a sale or ticket, restoring item quantities back to inventory stock.
    /// If fiado, reverses the client debtor balance.
    /// </summary>
    public async Task<string?> AnularVentaAsync(int ventaId, string motivo)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var venta = await _db.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
                return "Comprobante no encontrado.";

            if (venta.Estado == EstadoVenta.Anulada)
                return "El comprobante ya se encuentra anulado.";

            if (venta.ComprobanteDestinoId.HasValue)
                return "No se puede anular este ticket porque ya fue convertido a una Boleta oficial. Anule la Boleta final directamente.";

            venta.Estado = EstadoVenta.Anulada;
            venta.MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? "Anulado a solicitud del cliente / devolución" : motivo.Trim();

            // Return items back to stock
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _db.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.StockActual += detalle.Cantidad;
                }
            }

            // If fiado, reverse debt
            if (venta.EsFiado && venta.ClienteId.HasValue)
            {
                var cliente = await _db.Clientes.FindAsync(venta.ClienteId.Value);
                if (cliente != null)
                {
                    cliente.SaldoDeudor = Math.Max(0, cliente.SaldoDeudor - venta.Total);
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return null; // Success
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return $"Error al anular comprobante: {ex.Message}";
        }
    }
}
