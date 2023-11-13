namespace FORMULARIOAPI.Models
{
    public class Personas
    {
        public int Id { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }

        public long? NroIdentificacion { get; set; }

        public string? Email { get; set; }
        public string? TipoIdentificacion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string? Username { get; set; }
        public string? IdentificacionConcatenada { get; set; }

        public string? NombresApellidos { get; set; }
       
        public string? Token { get; set; }
        public string? Role { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
