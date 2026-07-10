using Front_End.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static Front_End.DTOs.TodoDtos;

namespace Front_End.Repository
{
    public class TodoRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string baseUrl;

        private readonly string Endpoint_Get; // Endpoint para obter todos os todos
        private readonly string Endpoint_Post; // Endpoint para criar um novo todo
        private readonly string Endpoint_Delete; // Endpoint para deletar um todo
        private readonly string Endpoint_Put; // Endpoint para atualizar um todo
        private readonly string Endpoint_Patch; // Endpoint para atualizar parcialmente um todo



        public TodoRepository(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            baseUrl = configuration.GetValue<string>("ServerAPI:BaseUrl");

            Endpoint_Get = $"{baseUrl}/todos";
            Endpoint_Post = $"{baseUrl}/todos";
            Endpoint_Delete = $"{baseUrl}/todos";
            Endpoint_Put = $"{baseUrl}/todos";
            Endpoint_Patch = $"{baseUrl}/todos";
        }

        // Método para obter todos os todos com paginação
        public async Task<PagedTodoResponse?> GetTodosAsync(string token, int page, int limit)
        {
            // Cria o cliente HTTP
            var client = _httpClientFactory.CreateClient();

            // Adiciona o token no cabeçalho Authorization (Bearer)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Monta a URL com os parâmetros de paginação
            var url = $"{Endpoint_Get}?page={page}&limit={limit}";

            // Faz a requisição GET
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                // 5. Desserializa o JSON respeitando o padrão CamelCase (id, title, etc) para o record PagedTodoResponse
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Deserialize<PagedTodoResponse>(jsonString, options);

                return result;
            }

            // Caso dê erro (ex: 401 Unauthorized ou 500), retorna null para a controller tratar
            return null;
        }


        // Método para criar
        public async Task<TodoResponse?> PostTodosAsync(string token, CadastroDtos.CadastroRequest cadastroDtos)
        {
            // Cria o cliente HTTP
            var client = _httpClientFactory.CreateClient();

            // Adiciona o token no cabeçalho Authorization (Bearer)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Monta a URL
            var url = $"{Endpoint_Post}";

            // Converte o objeto cadastroDtos para JSON
            var jsonContent = new StringContent(JsonSerializer.Serialize(cadastroDtos), Encoding.UTF8, "application/json");

            // Faz a requisição POST
            var response = await client.PostAsync(url, jsonContent);

            // Desserializa o JSON respeitando o padrão CamelCase
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Deserialize<TodoResponse>(jsonString, options);
                return result;
            }

            return null;
        }

        // Método para deletar
        public async Task<bool> DeleteTodoAsync(string token, int id)
        {
            // Cria o cliente HTTP
            var client = _httpClientFactory.CreateClient();

            // Adiciona o token no cabeçalho Authorization (Bearer)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Monta a URL
            var url = $"{Endpoint_Delete}/{id}";

            // Faz a requisição DELETE
            var response = await client.DeleteAsync(url);

            if (response != null && response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }

        // Método para atualizar
        public async Task<TodoResponse?> UpdateTodoAsync(string token, int id, EditarDtos.EditarRequest editarDtos)
        {
            // Cria o cliente HTTP
            var client = _httpClientFactory.CreateClient();
            // Adiciona o token no cabeçalho Authorization (Bearer)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Monta a URL
            var url = $"{Endpoint_Put}/{id}";
            // Converte o objeto editarDtos para JSON
            var jsonContent = new StringContent(JsonSerializer.Serialize(editarDtos), Encoding.UTF8, "application/json");
            // Faz a requisição PUT
            var response = await client.PutAsync(url, jsonContent);
            // Desserializa o JSON respeitando o padrão CamelCase
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Deserialize<TodoResponse>(jsonString, options);
                return result;
            }
            return null;

        }


        public async Task<bool> UpdateCompleteTodoAsync(string token, int id)
        {
            // Cria o cliente HTTP
            var client = _httpClientFactory.CreateClient();

            // Adiciona o token no cabeçalho Authorization (Bearer)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Monta a URL
            var url = $"{Endpoint_Patch}/{id}/complete";

            // Faz a requisição PATCH
            var response = await client.PatchAsync(url, null);

            if (response != null && response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
    }
}
