namespace ECommerceJuegos.ViewModels.Api;

public class CompraApiDto
{
    public int OrdenId { get; set; }
    public int ProductoId { get; set; }
    public DateTime Fecha { get; set; }
    public string ProductoNombre { get; set; } = "";
    public decimal PrecioPagado { get; set; }
    public string CodigoClave { get; set; } = "";
    public string? PortadaRuta { get; set; }
}
