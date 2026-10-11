using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.Models;

public class ClaveDigital
{
    [Key]
    public int ClaveDigitalId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Required, StringLength(255)]
    public string Codigo { get; set; } = "";

    [Required, RegularExpression("^(Disponible|Vendida)$")]
    public string Estado { get; set; } = "Disponible";
}