using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.ViewModels;

public class UsuarioEditViewModel
{
    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";

    [Required, RegularExpression("^(Administrador|Cliente)$", ErrorMessage = "El rol no es válido.")]
    public string Rol { get; set; } = "Cliente";

    public bool EstadoActivo { get; set; }
}