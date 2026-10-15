namespace ECommerceJuegos.ViewModels
{
    public class ResenaViewModel
    {
        public int ResenaId { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioEmail { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; } = "";
        public DateTime Fecha { get; set; }
    }
}