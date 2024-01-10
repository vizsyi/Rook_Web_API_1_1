namespace Rook01_08.Models.Auth.DTOs
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public DateTime DateExpire { get; set; }
    }
}
