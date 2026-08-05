using PosBodega.Data;
using PosBodega.Models;
using Microsoft.EntityFrameworkCore;

namespace PosBodega.Services;

public class PagoFiadoService
{
    private readonly AppDbContext _db;

    public PagoFiadoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PagoFiado>> GetByClienteAsync(int clienteId)
    {
        return await _db.PagosFiado
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();
    }

    /// <summary>
    /// Registers a credit payment. Validates payment does not exceed total debt.
    /// Returns error message on failure, null on success.
    /// </summary>
    public async Task<string?> RegistrarPagoAsync(int clienteId, decimal monto, string? observaciones)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var cliente = await _db.Clientes.FindAsync(clienteId);
            if (cliente == null)
                return "Cliente no encontrado.";

            if (monto <= 0)
                return "El monto debe ser mayor a cero.";

            if (monto > cliente.SaldoDeudor)
                return $"El pago (S/ {monto:N2}) no puede ser mayor a la deuda total (S/ {cliente.SaldoDeudor:N2}).";

            var pago = new PagoFiado
            {
                ClienteId = clienteId,
                Fecha = DateTime.Now,
                Monto = monto,
                SaldoAnterior = cliente.SaldoDeudor,
                SaldoRestante = cliente.SaldoDeudor - monto,
                Observaciones = observaciones
            };

            _db.PagosFiado.Add(pago);
            cliente.SaldoDeudor -= monto;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return null;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return $"Error al registrar el pago: {ex.Message}";
        }
    }
}
