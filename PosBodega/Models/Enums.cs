namespace PosBodega.Models;

public enum Rol
{
    Administrador,
    Vendedor
}

public enum MetodoPago
{
    Efectivo,
    YapePlin,
    Tarjeta
}

public enum TipoComprobante
{
    Boleta,
    Factura,
    TicketInterno
}

public enum EstadoVenta
{
    Emitida,
    Anulada
}

public enum TipoDocumentoIdentidad
{
    SinDocumento,
    DNI,
    RUC
}

public enum UnidadMedida
{
    Unidad,
    Kilogramo,
    Litro
}

