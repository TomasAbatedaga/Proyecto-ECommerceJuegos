namespace ECommerceJuegos.ViewModels;

public class UsuariosIndexViewModel
{
    public IList<UsuarioResumenViewModel> Usuarios { get; set; } = new List<UsuarioResumenViewModel>();
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public bool TieneAnterior => PaginaActual > 1;
    public bool TieneSiguiente => PaginaActual < TotalPaginas;
}