using EncontreProfessor.Api.Controllers;
using EncontreProfessor.Api.Dados;
using EncontreProfessor.Dominio.Busca;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

const string CorsFrontend = "Frontend";

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    // Erros de model binding (ex.: duracaoMinutos enviado como texto, JSON malformado)
    // saem no mesmo formato dos erros de validação do domínio.
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState
            .Where(kv => kv.Value is { Errors.Count: > 0 })
            .SelectMany(kv => kv.Value!.Errors.Select(e =>
                string.IsNullOrWhiteSpace(kv.Key)
                    ? (string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Corpo da requisição inválido." : e.ErrorMessage)
                    : $"Campo '{kv.Key}': {(string.IsNullOrWhiteSpace(e.ErrorMessage) ? "valor inválido." : e.ErrorMessage)}"))
            .ToList();

        if (erros.Count == 0) erros.Add("Corpo da requisição inválido.");

        return new BadRequestObjectResult(new ErroDeValidacaoResponse("Requisição inválida.", erros));
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsFrontend, policy =>
    {
        var origens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>()
                      ?? new[] { "http://localhost:5173" };
        policy.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod();
    });
});

// Dados em memória, carregados uma vez do professores.json (caminho configurável).
var caminhoProfessores = builder.Configuration["Professores:Caminho"] ?? "professores.json";
if (!Path.IsPathRooted(caminhoProfessores))
    caminhoProfessores = Path.Combine(AppContext.BaseDirectory, caminhoProfessores);

builder.Services.AddSingleton<IRepositorioDeProfessores>(_ => new RepositorioDeProfessoresJson(caminhoProfessores));
builder.Services.AddSingleton<BuscaDeProfessores>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsFrontend);
app.MapControllers();

app.Run();

// Permite que testes de integração referenciem o ponto de entrada, se necessário.
public partial class Program { }
