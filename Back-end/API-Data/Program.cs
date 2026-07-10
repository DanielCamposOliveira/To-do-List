using API_Data.Data;
using API_Data.DTOs;
using API_Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using static API_Data.DTOs.TodoDtos;
//using static API_Data.DTOs.UserDtos;


var builder = WebApplication.CreateBuilder(args);

// 1. Recupera a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection");

// 2. Configura a Injeção de Dependência para o EF Core usar o PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));



// Serviços necessários para o Swagger funcionar
builder.Services.AddSwaggerGen();


// --- Configuração de Autenticação JWT ---
var jwtKey = builder.Configuration.GetSection("JWT:Key").Value;
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

// Configura a autenticação JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // tenho que mudar para true quando colocar em produção
    options.SaveToken = true;  // Salva o token no contexto da requisição

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // Valida a chave de assinatura do token
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false, // Não valida o emissor
        ValidateAudience = false, // Não valida o público
        ClockSkew = TimeSpan.Zero // isso significa que não vai aceita  tolerancia de tokem espirado, ex. 5min de atraso
    };

    options.Events = new JwtBearerEvents
    {
        // Quando a requisição chega
        OnMessageReceived = context =>
        {
            return Task.CompletedTask;
        },

        // Quando o token é validado com sucesso (bom para checar status no banco de dados)
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token aceito com sucesso!");
            return Task.CompletedTask;
        },

        // Quando a autenticação falha por causa do token (expirado, assinatura inválida, etc)
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Falha na autenticação: " + context.Exception.Message);
            return Task.CompletedTask;
        },

        // Quando o usuário NÃO envia token ou o token falha e o .NET vai barrar a requisição (401)
        OnChallenge = context =>
        {
            // Cancela o comportamento padrão de mandar uma resposta vazia
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var resultado = new { message = "Não autorizado. Você precisa enviar um token JWT válido no Header." };

            // Usamos .WriteAsJsonAsync(...) e retornamos a Task gerada por ele diretamente
            return context.Response.WriteAsJsonAsync(resultado);
        },

        // Quando o usuário está logado, mas tenta acessar algo que não tem direito (403)
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var resultado = new { message = "Você não tem permissão para acessar este recurso." };
            return context.Response.WriteAsJsonAsync(resultado);
        }
    };

});

// Configura a autorização
builder.Services.AddAuthorization();

// Adiciona os serviços para a Minimal API mapear endpoints
builder.Services.AddEndpointsApiExplorer();


// Configura o Rate Limiting por IP de origem
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // essa configuração define que cada IP pode fazer no máximo 10 requisições a cada 10 segundos. Se passar disso, recebe 429 Too Many Requests.
    options.AddPolicy("IpLimitPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,  // Permite que o contador de requisições seja reiniciado automaticamente após o período definido
                PermitLimit = 10, // Máximo de 10 requisições...
                Window = TimeSpan.FromSeconds(10), // ...a cada 10 segundos
                QueueLimit = 0
            }));
});


// Configurações do Swagger para suportar autenticação JWT - mostra o botão "Authorize" no Swagger UI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Info Host API", Version = "v1" });

    // 1. Define o esquema de segurança Bearer para o Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT"
    });

    // 2. Aplica essa exigência de segurança globalmente ou para as rotas protegidas no Swagger
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Ative o middleware do Rate Limiter (coloque antes de mapear as rotas)
app.UseRateLimiter();


app.UseHttpsRedirection();

// ATENÇÃO: A ordem importa! Autenticação antes de Autorização.
app.UseAuthentication();
app.UseAuthorization();



// Configurações do Swagger no ambiente de Desenvolvimento
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Info Host API v1");
        // Se quiser que o Swagger abra digitando apenas http://localhost:8585/, deixe a linha abaixo.
        // Se preferir acessar por http://localhost:8585/swagger, comente a linha abaixo com //
        c.RoutePrefix = string.Empty;
    });
//}


// Método auxiliar para gerar tokens JWT
string GenerateJwtToken(User user)
{
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        // Aqui você pode adicionar mais claims se necessário, como roles, permissões, etc.
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }),

        // Define a expiração do token  aqui eu coloquei 2 hrs
        Expires = DateTime.UtcNow.AddHours(2),

        // Define a chave de assinatura e o algoritmo
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
    };

    // Cria o token e retorna como string
    var token = tokenHandler.CreateToken(tokenDescriptor);

    return tokenHandler.WriteToken(token);
}




// Grupo de rotas para /todos, todas as rotas dentro deste grupo exigem autenticação
//var todoRoutes = app.MapGroup("/todos").RequireAuthorization();
// Adiciona Rate Limiting para o grupo de rotas /todos
var todoRoutes = app.MapGroup("/todos").RequireAuthorization().RequireRateLimiting("IpLimitPolicy");

// Este método extrai o ID do usuário a partir das claims do token JWT
int GetUserId(ClaimsPrincipal userPrincipal) =>
    // Tenta pegar o Claim do tipo NameIdentifier (que é onde armazenamos o ID do usuário no token)
    int.Parse(userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");





// POST /register
//app.MapPost("/register", async (Microsoft.AspNetCore.Identity.Data.RegisterRequest request, AppDbContext db) =>
app.MapPost("/register", async (UserDtos.RegisterRequest request, AppDbContext db) =>
{
    try
    {
        // Validação básica
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest(new { message = "E-mail e senha são obrigatórios." });

        // Garantir e-mail único
        var userExists = await db.Users.AnyAsync(u => u.Email == request.Email);
        if (userExists)
            return Results.BadRequest(new { message = "E-mail já cadastrado." });

        // Criptografar senha
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Monta o pacote de dados do usuário para salvar no banco
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Password = hashedPassword
        };


        // Adiciona o usuário ao banco de dados
        db.Users.Add(user);

        // Salva as alterações de forma assíncrona
        await db.SaveChangesAsync();

        // Gera o token JWT para o usuário recém-criado
        var token = GenerateJwtToken(user);

        return Results.Ok(new UserDtos.AuthResponse(token)); // Retorna o token em caso de sucesso
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao criar usuário: {ex.Message}");
    }
}).WithSummary("Registrar usuário")
.WithDescription("Cria um novo usuário e retorna um token JWT.");


// POST /login
//app.MapPost("/login", async (Microsoft.AspNetCore.Identity.Data.LoginRequest request, AppDbContext db) =>
app.MapPost("/login", async (UserDtos.LoginRequest request, AppDbContext db) =>
{
    try
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Results.Json(new { message = "E-mail ou senha inválidos." }, statusCode: 401);
        }

        // Gera o token JWT para o usuário logado 
        var token = GenerateJwtToken(user);

        return Results.Ok(new UserDtos.AuthResponse(token)); // Retorna o token em caso de sucesso
    }
    catch
    {
        return Results.Problem("Erro ao autenticar usuário.");
    }

}).WithSummary("Login")
.WithDescription("Autentica o usuário e retorna um token JWT.");


// GET /todos?page=1&limit=10
todoRoutes.MapGet("/", async (ClaimsPrincipal userPrincipal, AppDbContext db, int page = 1, int limit = 10) =>
{
    try
    {
        int userId = GetUserId(userPrincipal);

        // Garantir paginação mínima válida
        if (page < 1) page = 1;
        if (limit < 1 || limit > 50) limit = 10;

        // Consulta base filtrando pelo usuário logado
        var query = db.Tasks.Where(t => t.UserId == userId);

        int totalItems = await query.CountAsync();

        // Paginação
        var tasks = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(t => new TodoResponse(t.Id, t.Title, t.Description, t.Completed))
            .ToListAsync();

        return Results.Ok(new PagedTodoResponse(tasks, page, limit, totalItems));
    }
    catch
    {
        return Results.Problem($"Erro ao listar tarefas");
    }

}).WithSummary("Lista tarefas paginadas")
.WithDescription("Retorna uma lista paginada de tarefas do usuário logado.");


// POST /todos
todoRoutes.MapPost("/", async (CreateTodoRequest request, ClaimsPrincipal userPrincipal, AppDbContext db) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { message = "O título é obrigatório." });

        int userId = GetUserId(userPrincipal);

        var newTask = new TaskModel
        {
            Title = request.Title,
            Description = request.Description,
            UserId = userId,
            Completed = false
        };

        db.Tasks.Add(newTask);
        await db.SaveChangesAsync();

        return Results.Json(new TodoResponse(newTask.Id, newTask.Title, newTask.Description, newTask.Completed), statusCode: 201);
    }
    catch
    {
        return Results.Problem($"Erro ao criar tarefa");
    }


}).WithSummary("Cria uma nova tarefa")
.WithDescription("Cria uma nova tarefa para o usuário logado.");


// PUT /todos/{id}
todoRoutes.MapPut("/{id:int}", async (int id, UpdateTodoRequest request, ClaimsPrincipal userPrincipal, AppDbContext db) =>
{
    try
    {
        var task = await db.Tasks.FindAsync(id);
        if (task == null) return Results.NotFound(new { message = "Tarefa não encontrada." });

        int userId = GetUserId(userPrincipal);

        // Validar se o usuário é o dono da tarefa (Regra 403 Forbidden)[cite: 1]
        if (task.UserId != userId)
            return Results.Json(new { message = "Forbidden" }, statusCode: 403);

        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { message = "O título não pode ser vazio." });

        task.Title = request.Title;
        task.Description = request.Description;

        await db.SaveChangesAsync();

        return Results.Ok(new TodoResponse(task.Id, task.Title, task.Description, task.Completed));
    }
    catch
    {
        return Results.Problem($"Erro ao atualizar tarefa");
    }


}).WithSummary("Editar tarefa")
.WithDescription("Atualiza o título e a descrição da tarefa informada no ID. Apenas o dono da tarefa pode alterá-la.");


// PATCH /todos/{id}/complete
todoRoutes.MapPatch("/{id:int}/complete", async (int id, ClaimsPrincipal userPrincipal, AppDbContext db) =>
{
    try
    {
        var task = await db.Tasks.FindAsync(id);
        if (task == null)
            return Results.NotFound(new { message = "Tarefa não encontrada." });

        int userId = GetUserId(userPrincipal);

        // Valida se o usuário logado é o dono da tarefa
        if (task.UserId != userId)
            return Results.Json(new { message = "Forbidden" }, statusCode: 403);

        // Inverte o status atual (se mapeado como bool no seu banco)
        // Nota: Certifique-se de que sua TaskModel tenha a propriedade IsCompleted ou Completed
        task.Completed = !task.Completed;

        await db.SaveChangesAsync();

        // Você pode retornar o objeto atualizado ou apenas uma mensagem de sucesso
        return Results.Ok(new { message = $"Tarefa marcada como {(task.Completed ? "concluída" : "pendente")}." });
    }
    catch
    {
        return Results.Problem($"Erro ao atualizar status da tarefa");
    }


}).WithSummary("Conclui ou reativa uma tarefa")
.WithDescription("Inverte o status de conclusão (IsCompleted) da tarefa informada no ID. Apenas o dono da tarefa pode alterá-la.");


// Delete /todos/{id}
todoRoutes.MapDelete("/{id:int}", async (int id, ClaimsPrincipal userPrincipal, AppDbContext db) =>
{
    try
    {
        var task = await db.Tasks.FindAsync(id);
        if (task == null) return Results.NotFound(new { message = "Tarefa não encontrada." });

        int userId = GetUserId(userPrincipal);

        // Validar se o usuário é o dono da tarefa antes de excluir[cite: 1]
        if (task.UserId != userId)
            return Results.Json(new { message = "Forbidden" }, statusCode: 403);

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch
    {
        return Results.Problem($"Erro ao excluir tarefa");
    }


}).WithSummary("Excluir tarefa")
.WithDescription("Exclui a tarefa informada no ID. Apenas o dono da tarefa pode excluí-la.");



app.Run();
