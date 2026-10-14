using System.ComponentModel.DataAnnotations;
namespace ECommerceJuegos.ViewModels;

public class ComprarProductoViewModel
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }
}
