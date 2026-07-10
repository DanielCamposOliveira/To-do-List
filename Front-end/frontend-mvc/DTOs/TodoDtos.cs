namespace Front_End.DTOs
{
    public class TodoDtos
    {
        public record TodoResponse(int Id, string Title, string? Description, bool Completed); // Resposta de tarefa
        public record PagedTodoResponse(List<TodoResponse> Data, int Page, int Limit, int Total); // Resposta paginada de tarefas
    }

    public class CadastroDtos
    {
        // Envio para a API
        public record CadastroRequest(string title, string description);
        // Resposta recebida da API
        public record TodoResponse(int Id, string Title, string? Description, bool? Completed); // Resposta de tarefa
    }

    public class EditarDtos
    {
        // Envio para a API
        public record EditarRequest(string title, string description);
        // Resposta recebida da API
        public record TodoResponse(int Id, string Title, string? Description, bool? Completed); // Resposta de tarefa
    }

    public record ErrorResponse(string Message);

    // DTOs para autenticação

    public class AuthDtos
    {
        // Envio para a API
        public record LoginRequest(string Email, string Password);

        // Resposta recebida da API
        public record LoginResponse(string Token);
    }

    public class CadastroUserDtos
    {
        // Envio para a API
        public record CadastroRequest(string Name, string Email, string Password);
        // Resposta recebida da API
        public record CadastroResponse(string Message);
    }



}
