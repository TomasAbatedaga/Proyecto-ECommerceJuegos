using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.Models;

public class Orden
{
    [Key]
    public int OrdenId { get; set; }

    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    public DateTime Fecha { get; set; }

    [Required, RegularExpression("^(Pendiente|Pagada|Cancelada)$")]
    public string Estado { get; set; } = "Pendiente";

    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal MontoTotal { get; set; }
}