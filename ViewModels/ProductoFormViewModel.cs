using System.ComponentModel.DataAnnotations;
using ECommerceJuegos.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace ECommerceJuegos.ViewModels;

public class ProductoFormViewModel
{
    public int ProductoId { get; set; }
    [Required(ErrorMessage = "Ingresá un nombre."), StringLength(150)]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "Ingresá una descripción."), StringLength(4000)]
    public string Descripcion { get; set; } = "";
    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Precio { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una categoría válida.")]
    public int CategoriaId { get; set; }
    public bool EstadoActivo { get; set; } = true;
    [BindNever] public string? PortadaRutaActual { get; set; }
    public IFormFile? Portada { get; set; }
    [BindNever] public IList<Categoria> Categorias { get; set; } = new List<Categoria>();
}
