using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.Models;

public class Producto
{
    [Key]
    public int ProductoId { get; set; }

    [Required, StringLength(150)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(4000)]
    public string Descripcion { get; set; } = "";

    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Precio { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoriaId { get; set; }

    public bool EstadoActivo { get; set; } = true;

    [StringLength(255)]
    public string? PortadaRuta { get; set; }
}