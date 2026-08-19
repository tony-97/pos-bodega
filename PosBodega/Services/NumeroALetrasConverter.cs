using System.Globalization;

namespace PosBodega.Services;

public static class NumeroALetrasConverter
{
    private static readonly string[] Unidades =
    {
        "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
        "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE",
        "DIECIOCHO", "DIECINUEVE", "VEINTE", "VEINTIUN", "VEINTIDOS", "VEINTITRES",
        "VEINTICUATRO", "VEINTICINCO", "VEINTISEIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE"
    };

    private static readonly string[] Decenas =
    {
        "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
    };

    private static readonly string[] Centenas =
    {
        "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS",
        "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
    };

    public static string Convertir(decimal monto, string moneda = "SOLES")
    {
        long entero = (long)Math.Floor(monto);
        int centavos = (int)Math.Round((monto - entero) * 100);

        string letras = entero == 0 ? "CERO" : ConvertirNumero(entero);
        return $"SON: {letras} CON {centavos:D2}/100 {moneda}".ToUpper(new CultureInfo("es-PE"));
    }

    private static string ConvertirNumero(long n)
    {
        if (n == 0) return "CERO";
        if (n == 100) return "CIEN";

        if (n < 30)
        {
            return Unidades[n];
        }

        if (n < 100)
        {
            long u = n % 10;
            long d = n / 10;
            return u == 0 ? Decenas[d] : $"{Decenas[d]} Y {Unidades[u]}";
        }

        if (n < 1000)
        {
            long c = n / 100;
            long resto = n % 100;
            if (resto == 0)
            {
                return c == 1 ? "CIEN" : Centenas[c];
            }
            return $"{Centenas[c]} {ConvertirNumero(resto)}";
        }

        if (n < 1000000)
        {
            long miles = n / 1000;
            long resto = n % 1000;
            string prefix = miles == 1 ? "MIL" : $"{ConvertirNumero(miles)} MIL";
            if (resto == 0) return prefix;
            return $"{prefix} {ConvertirNumero(resto)}";
        }

        if (n < 1000000000)
        {
            long millones = n / 1000000;
            long resto = n % 1000000;
            string prefix = millones == 1 ? "UN MILLON" : $"{ConvertirNumero(millones)} MILLONES";
            if (resto == 0) return prefix;
            return $"{prefix} {ConvertirNumero(resto)}";
        }

        return n.ToString();
    }
}
