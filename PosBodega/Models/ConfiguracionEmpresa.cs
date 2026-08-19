using System.ComponentModel.DataAnnotations;

namespace PosBodega.Models;

public class ConfiguracionEmpresa
{
    public int Id { get; set; } = 1;

    [Required, MaxLength(11)]
    public string Ruc { get; set; } = "10456789123";

    [Required, MaxLength(200)]
    public string RazonSocial { get; set; } = "BODEGA SAN JUAN";

    [MaxLength(200)]
    public string NombreComercial { get; set; } = "Bodega San Juan - Abarrotes & Bebidas";

    [MaxLength(200)]
    public string Titular { get; set; } = "Juan Carlos Mendoza Pérez";

    [MaxLength(300)]
    public string Direccion { get; set; } = "Jr. Dos de Mayo N° 456, Huánuco";

    [MaxLength(50)]
    public string Telefono { get; set; } = "(062) 512345 / 962123456";

    [MaxLength(100)]
    public string RegimenTributario { get; set; } = "Nuevo Régimen Único Simplificado (Nuevo RUS)";

    [MaxLength(10)]
    public string SerieBoleta { get; set; } = "B001";

    [MaxLength(10)]
    public string SerieFactura { get; set; } = "F001";

    [MaxLength(10)]
    public string SerieTicket { get; set; } = "T001";

    [MaxLength(300)]
    public string MensajePie { get; set; } = "¡Gracias por su preferencia! Vuelva pronto.";
}
