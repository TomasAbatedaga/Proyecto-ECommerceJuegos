namespace ECommerceJuegos.Models;

public enum EstadoResultadoCompra
{
    Exito, ProductoNoEncontrado, ProductoInactivo, UsuarioInvalido,
    SinStock, ErrorTemporal, Error
}

public record ResultadoCompra(EstadoResultadoCompra Estado, string Mensaje, int? OrdenId = null);
