using System.Text.Json;
using System.Text.Json.Serialization;
using EncontreProfessor.Dominio.Modelos;

namespace EncontreProfessor.Api.Dados;

public interface IRepositorioDeProfessores
{
    IReadOnlyList<Professor> ObterTodos();
}

/// <summary>
/// Carrega <c>professores.json</c> uma única vez na inicialização e mantém os dados em memória.
/// O arquivo é desserializado em DTOs com strings e só então convertido para o modelo de domínio
/// (com <see cref="Horario"/> tipado), para que um horário malformado no arquivo falhe cedo e com
/// mensagem clara.
/// </summary>
public sealed class RepositorioDeProfessoresJson : IRepositorioDeProfessores
{
    private readonly IReadOnlyList<Professor> _professores;

    public RepositorioDeProfessoresJson(string caminhoArquivo)
    {
        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException($"Arquivo de professores não encontrado: {caminhoArquivo}", caminhoArquivo);

        var json = File.ReadAllText(caminhoArquivo);
        var opcoes = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        var arquivo = JsonSerializer.Deserialize<ArquivoDeProfessores>(json, opcoes)
                      ?? throw new InvalidDataException("professores.json está vazio ou inválido.");

        _professores = (arquivo.Professores ?? new List<ProfessorDto>())
            .Select(Converter)
            .ToList();
    }

    public IReadOnlyList<Professor> ObterTodos() => _professores;

    private static Professor Converter(ProfessorDto dto)
    {
        var disponibilidade = (dto.Disponibilidade ?? new())
            .Select(b => new BlocoDisponibilidade(
                DiaOuErro(b.Dia, dto),
                Horario.Parse(b.Inicio ?? ""),
                Horario.Parse(b.Fim ?? "")))
            .ToList();

        var aulas = (dto.AulasMarcadas ?? new())
            .Select(a => new AulaMarcada(
                DiaOuErro(a.Dia, dto),
                Horario.Parse(a.Inicio ?? ""),
                a.DuracaoMinutos))
            .ToList();

        return new Professor(
            dto.Id,
            dto.Nome ?? $"Professor {dto.Id}",
            dto.Idiomas ?? new List<string>(),
            dto.AgendaBloqueada,
            disponibilidade,
            aulas);
    }

    private static string DiaOuErro(string? dia, ProfessorDto dto) =>
        DiaDaSemana.Normalizar(dia)
        ?? throw new InvalidDataException($"Dia inválido '{dia}' no professor {dto.Id} ({dto.Nome}).");

    // ---- DTOs espelhando o formato do arquivo ----

    private sealed class ArquivoDeProfessores
    {
        [JsonPropertyName("professores")] public List<ProfessorDto>? Professores { get; set; }
    }

    private sealed class ProfessorDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("nome")] public string? Nome { get; set; }
        [JsonPropertyName("idiomas")] public List<string>? Idiomas { get; set; }
        [JsonPropertyName("agendaBloqueada")] public bool AgendaBloqueada { get; set; }
        [JsonPropertyName("disponibilidade")] public List<BlocoDto>? Disponibilidade { get; set; }
        [JsonPropertyName("aulasMarcadas")] public List<AulaDto>? AulasMarcadas { get; set; }
    }

    private sealed class BlocoDto
    {
        [JsonPropertyName("dia")] public string? Dia { get; set; }
        [JsonPropertyName("inicio")] public string? Inicio { get; set; }
        [JsonPropertyName("fim")] public string? Fim { get; set; }
    }

    private sealed class AulaDto
    {
        [JsonPropertyName("dia")] public string? Dia { get; set; }
        [JsonPropertyName("inicio")] public string? Inicio { get; set; }
        [JsonPropertyName("duracaoMinutos")] public int DuracaoMinutos { get; set; }
    }
}
