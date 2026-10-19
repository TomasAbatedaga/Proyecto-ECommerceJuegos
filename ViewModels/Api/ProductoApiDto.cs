namespace ECommerceJuegos.ViewModels.Api;

public class ProductoApiDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Precio { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = "";
    public string? PortadaRuta { get; set; }
    public bool TieneStock { get; set; }
}
