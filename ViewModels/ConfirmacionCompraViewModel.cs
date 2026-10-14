namespace ECommerceJuegos.ViewModels;

public class ConfirmacionCompraViewModel
{
    public int OrdenId { get; set; }
    public DateTime Fecha { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public decimal PrecioPagado { get; set; }
    public string CodigoClave { get; set; } = "";
}
