using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.Models;

public class Resena
{
    [Key]
    public int ResenaId { get; set; }

    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(1, 5)]
    public int Puntuacion { get; set; }

    [Required, StringLength(2000)]
    public string Comentario { get; set; } = "";

    public DateTime Fecha { get; set; }
}