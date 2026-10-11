using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.Models;

public class Usuario
{
    [Key]
    public int UsuarioId { get; set; }

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";

    [Required, StringLength(255)]
    public string ContraseniaHash { get; set; } = "";

    [Required, RegularExpression("^(Administrador|Cliente)$")]
    public string Rol { get; set; } = "";

    public string? AvatarUrl { get; set; }

    public bool EstadoActivo { get; set; } = true;
}