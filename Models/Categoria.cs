using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.Models;

public class Categoria
{
    [Key]
    public int CategoriaId { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";
}