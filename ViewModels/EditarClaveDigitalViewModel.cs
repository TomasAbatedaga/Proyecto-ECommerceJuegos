using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.ViewModels;

public class EditarClaveDigitalViewModel
{
    [Range(1, int.MaxValue)]
    public int ClaveDigitalId { get; set; }

    [Required, StringLength(255)]
    public string Codigo { get; set; } = "";
}
