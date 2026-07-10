namespace API_Data.DTOs
{ 
    public class UserDtos
    {
        public record RegisterRequest(string Name, string Email, string Password); // Requisição de registro
        public record LoginRequest(string Email, string Password); // Requisição de login
        public record AuthResponse(string Token); // Retorna o token JWT
    }
}
