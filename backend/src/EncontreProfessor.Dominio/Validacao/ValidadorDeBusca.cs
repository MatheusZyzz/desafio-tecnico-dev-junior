using EncontreProfessor.Dominio.Busca;
using EncontreProfessor.Dominio.Modelos;

namespace EncontreProfessor.Dominio.Validacao;

/// <summary>
/// Requisição de busca crua, exatamente como chega da camada HTTP (tudo opcional,
/// para que a validação de negócio — e não o model binding — produza as mensagens).
/// </summary>
public sealed record RequisicaoDeBusca(
    string? Idioma,
    IReadOnlyList<string?>? Dias,
    string? InicioFaixa,
    string? FimFaixa,
    int? DuracaoMinutos);

public sealed record ResultadoDaValidacao(CriteriosDeBusca? Criterios, IReadOnlyList<string> Erros)
{
    public bool EhValido => Erros.Count == 0 && Criterios is not null;
}

/// <summary>Validação da seção 3 do desafio. Acumula todos os erros em vez de parar no primeiro.</summary>
public static class ValidadorDeBusca
{
    public static ResultadoDaValidacao Validar(RequisicaoDeBusca? requisicao)
    {
        var erros = new List<string>();

        if (requisicao is null)
        {
            erros.Add("O corpo da requisição é obrigatório.");
            return new ResultadoDaValidacao(null, erros);
        }

        // Idioma
        var idioma = requisicao.Idioma?.Trim();
        if (string.IsNullOrEmpty(idioma))
            erros.Add("O idioma é obrigatório.");

        // Dias
        var dias = new List<string>();
        if (requisicao.Dias is null || requisicao.Dias.Count == 0)
        {
            erros.Add("Informe pelo menos um dia da semana.");
        }
        else
        {
            foreach (var dia in requisicao.Dias)
            {
                var normalizado = DiaDaSemana.Normalizar(dia);
                if (normalizado is null)
                    erros.Add($"Dia inválido: '{dia}'. Use: {string.Join(", ", DiaDaSemana.Todos)}.");
                else
                    dias.Add(normalizado);
            }
        }

        // Horários
        var inicioOk = Horario.TryParse(requisicao.InicioFaixa, out var inicio);
        if (!inicioOk)
            erros.Add($"Início da faixa inválido: '{requisicao.InicioFaixa}'. Use o formato HH:mm.");

        var fimOk = Horario.TryParse(requisicao.FimFaixa, out var fim);
        if (!fimOk)
            erros.Add($"Fim da faixa inválido: '{requisicao.FimFaixa}'. Use o formato HH:mm.");

        if (inicioOk && fimOk && fim <= inicio)
            erros.Add("O fim da faixa deve ser posterior ao início.");

        // Duração
        if (requisicao.DuracaoMinutos is null ||
            !CriteriosDeBusca.DuracoesPermitidas.Contains(requisicao.DuracaoMinutos.Value))
        {
            erros.Add("A duração deve ser 30, 60 ou 90 minutos.");
        }

        if (erros.Count > 0)
            return new ResultadoDaValidacao(null, erros);

        var criterios = new CriteriosDeBusca(
            idioma!,
            dias.Distinct(StringComparer.Ordinal).ToList(),
            inicio,
            fim,
            requisicao.DuracaoMinutos!.Value);

        return new ResultadoDaValidacao(criterios, erros);
    }
}
