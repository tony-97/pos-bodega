using System.Text.RegularExpressions;
using PosBodega.Models;

namespace PosBodega.Services;

public static class SunatValidator
{
    private static readonly int[] RucWeights = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

    /// <summary>
    /// Validates Peruvian DNI (must be exactly 8 numeric digits).
    /// </summary>
    public static bool EsDniValido(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return false;
        var clean = dni.Trim();
        return clean.Length == 8 && clean.All(char.IsDigit);
    }

    /// <summary>
    /// Validates Peruvian SUNAT RUC (must be 11 digits, valid prefix, and pass Modulo 11 check).
    /// </summary>
    public static bool EsRucValido(string? ruc, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(ruc))
        {
            error = "El número de RUC no puede estar vacío.";
            return false;
        }

        var clean = ruc.Trim();

        if (clean.Length == 8)
        {
            error = "Ha ingresado un DNI (8 dígitos), no un RUC (11 dígitos).";
            return false;
        }

        if (clean.Length != 11 || !clean.All(char.IsDigit))
        {
            error = "El RUC debe tener exactamente 11 dígitos numéricos.";
            return false;
        }

        // Check valid prefixes: 10 (persona natural), 15, 17, 20 (persona jurídica), 16
        var prefix = clean.Substring(0, 2);
        if (prefix != "10" && prefix != "15" && prefix != "16" && prefix != "17" && prefix != "20")
        {
            error = $"El RUC ingresado tiene un prefijo inválido ({prefix}). Debe iniciar con 10 o 20.";
            return false;
        }

        // Modulo 11 check
        int sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += (clean[i] - '0') * RucWeights[i];
        }

        int remainder = sum % 11;
        int checkDigit = 11 - remainder;
        if (checkDigit == 10) checkDigit = 0;
        else if (checkDigit == 11) checkDigit = 1;

        int expectedCheck = clean[10] - '0';
        if (checkDigit != expectedCheck)
        {
            error = "El RUC ingresado no es válido según el dígito verificador de SUNAT.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates full document requirements for SUNAT compliance under the Nuevo RUS regime.
    /// Under Nuevo RUS (persona natural RUC 10), businesses only emit Boletas de Venta and internal tickets.
    /// </summary>
    public static bool ValidarComprobante(
        TipoComprobante tipo,
        decimal total,
        string? clienteNombre,
        string? clienteDocumento,
        out string mensajeError)
    {
        mensajeError = string.Empty;

        if (tipo == TipoComprobante.Boleta)
        {
            if (total > 700)
            {
                if (string.IsNullOrWhiteSpace(clienteDocumento))
                {
                    mensajeError = "Por norma SUNAT, para ventas mayores a S/ 700.00 es obligatorio identificar al cliente con DNI o RUC.";
                    return false;
                }

                var cleanDoc = clienteDocumento.Trim();
                if (!EsDniValido(cleanDoc) && !EsRucValido(cleanDoc, out var rucErr))
                {
                    mensajeError = $"El documento del cliente para montos mayores a S/ 700 debe ser un DNI válido (8 dígitos) o RUC (11 dígitos). {rucErr}".Trim();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(clienteNombre) || clienteNombre.Trim().Equals("Clientes Varios", StringComparison.OrdinalIgnoreCase))
                {
                    mensajeError = "Por norma SUNAT, para ventas mayores a S/ 700.00 debe ingresar los Apellidos y Nombres o Razón Social del cliente.";
                    return false;
                }
            }
            else if (!string.IsNullOrWhiteSpace(clienteDocumento))
            {
                var cleanDoc = clienteDocumento.Trim();
                if (!EsDniValido(cleanDoc) && !EsRucValido(cleanDoc, out var rucErr))
                {
                    mensajeError = $"Documento inválido: {rucErr}";
                    return false;
                }
            }
        }

        return true;
    }
}
