using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Contrasenia { get; set; } = "";

        [Required]
        public string Rol { get; set; } = "";

        public string? AvatarUrl { get; set; }
    }
}