namespace API_Data.DTOs
{
    public class TodoDtos
    {
        public record CreateTodoRequest(string Title, string? Description); // Requisição de criação de tarefa
        public record UpdateTodoRequest(string Title, string? Description); // Requisição de atualização de tarefa
        public record TodoResponse(int Id, string Title, string? Description, bool? Completed); // Resposta de tarefa
        public record PagedTodoResponse(List<TodoResponse> Data, int Page, int Limit, int Total); // Resposta paginada de tarefas
    }
}

