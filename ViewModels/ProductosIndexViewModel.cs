namespace ECommerceJuegos.ViewModels;

public class ProductosIndexViewModel
{
    public IList<ProductoListadoViewModel> Productos { get; set; } = new List<ProductoListadoViewModel>();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public bool TieneAnterior => PaginaActual > 1;
    public bool TieneSiguiente => PaginaActual < TotalPaginas;
}
