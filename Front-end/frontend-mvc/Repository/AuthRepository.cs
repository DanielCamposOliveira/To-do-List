using Front_End.Controllers;
using Front_End.DTOs;
using System.Text;
using System.Text.Json;
using static Front_End.DTOs.AuthDtos;

namespace Front_End.Repository
{
    public class AuthRepository
    {
        private readonly ILogger<AuthRepository> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthRepository(ILogger<AuthRepository> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var client = _httpClientFactory.CreateClient();

            var loginData = new LoginRequest(email, password);
            var jsonBody = JsonSerializer.Serialize(loginData);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Endpoint de login da sua API
            var response = await client.PostAsync("http://localhost:8000/login", content);
            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<LoginResponse>(jsonString, options);
            }

            // Se chegou aqui, deu erro (Ex: 400 ou 401)
            try
            {
                // Tenta desserializar a mensagem de erro da API
                var errorObj = JsonSerializer.Deserialize<ErrorResponse>(jsonString, options);

                if (errorObj != null && !string.IsNullOrEmpty(errorObj.Message))
                {
                    // Lança a exceção com a mensagem exata recebida: "E-mail ou senha inválidos."
                    throw new HttpRequestException(errorObj.Message);

                  
                }
            }
            catch (JsonException ex)
            {
                // Caso o retorno não seja o JSON esperado, envia uma mensagem genérica
                _logger.LogError(ex, "Erro ao desserializar a resposta de erro da API.");
                throw new HttpRequestException("Falha na autenticação com o servidor.");
            }


            throw new HttpRequestException("Falha na autenticação com o servidor.");


      
        }
    }
}
