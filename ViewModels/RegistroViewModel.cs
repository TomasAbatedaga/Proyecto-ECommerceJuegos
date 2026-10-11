using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.ViewModels;

public class RegistroViewModel
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";

    [Required, StringLength(128, MinimumLength = 8), DataType(DataType.Password)]
    public string Contrasenia { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Contrasenia), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarContrasenia { get; set; } = "";
}