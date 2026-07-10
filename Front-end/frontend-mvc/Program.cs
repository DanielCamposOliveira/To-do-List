using Front_End.Repository;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//*** Configura o Data Protection para persistir as chaves em um diretório específico
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\ProgramData\MeuWeb\Keys\"))
//    .SetApplicationName("MeuWebBackupApp");


// Adicione estas linhas antes do builder.Build();
builder.Services.AddHttpClient();
builder.Services.AddScoped<TodoRepository>();
builder.Services.AddScoped<AuthRepository>();


// Configura o CORS para permitir requisições de qualquer origem, cabeçalho e método
builder.Services.AddCors(options =>
{
    options.AddPolicy("DesenvolvimentoTotal", policy =>
    {
        policy.AllowAnyOrigin()   // Permite QUALQUER porta, IP ou domínio
              .AllowAnyHeader()   // Permite qualquer cabeçalho extra
              .AllowAnyMethod();  // Permite GET, POST, PUT, DELETE, etc.
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Index");
}

// intercepta erros de status como 404 e 500 e reexecuta a rota. para pagina NotFound
app.UseStatusCodePagesWithReExecute("/Error/Index", "?statusCode={0}");


app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
