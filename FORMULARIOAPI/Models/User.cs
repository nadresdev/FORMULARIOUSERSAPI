namespace FORMULARIOAPI.Models
{
    public class User
    {
        public int Id { get; set; }
    
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
