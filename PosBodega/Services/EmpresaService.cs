using PosBodega.Models;

namespace PosBodega.Services;

public class EmpresaService
{
    private ConfiguracionEmpresa _config = new()
    {
        Id = 1,
        Ruc = "10456789124",
        RazonSocial = "BODEGA SAN JUAN",
        NombreComercial = "Bodega San Juan - Abarrotes y Bebidas",
        Titular = "Juan Carlos Mendoza Pérez",
        Direccion = "Jr. Dos de Mayo N° 456, Huánuco",
        Telefono = "(062) 512345 / 962123456",
        RegimenTributario = "Nuevo Régimen Único Simplificado (Nuevo RUS)",
        SerieBoleta = "B001",
        SerieTicket = "T001",
        MensajePie = "¡Gracias por su compra! Vuelva pronto."
    };

    public ConfiguracionEmpresa GetConfiguracion() => _config;

    public void UpdateConfiguracion(ConfiguracionEmpresa config)
    {
        _config = config;
    }
}
