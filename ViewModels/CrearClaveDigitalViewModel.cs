using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.ViewModels;

public class CrearClaveDigitalViewModel
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Required, StringLength(255)]
    public string Codigo { get; set; } = "";
}
