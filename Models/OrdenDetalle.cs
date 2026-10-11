using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.Models;

public class OrdenDetalle
{
    [Key]
    public int OrdenDetalleId { get; set; }

    [Range(1, int.MaxValue)]
    public int OrdenId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue)]
    public int ClaveDigitalId { get; set; }

    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal PrecioUnitario { get; set; }
}