using System.Globalization;
using EncontreProfessor.Dominio.Modelos;

namespace EncontreProfessor.Dominio.Busca;

/// <summary>
/// Implementa as regras da busca (seção 2 do desafio). Não conhece HTTP nem JSON:
/// recebe critérios tipados e a lista de professores e devolve o resultado ordenado.
/// </summary>
public sealed class BuscaDeProfessores
{
    private const int PassoEmMinutos = 30;

    private static readonly CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;

    // Comparação de idioma sem diferenciar maiúsculas nem acentos ("ingles" == "Inglês").
    private const CompareOptions OpcoesIdioma = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    public ResultadoDaBusca Buscar(CriteriosDeBusca criterios, IEnumerable<Professor> professores)
    {
        ArgumentNullException.ThrowIfNull(criterios);
        ArgumentNullException.ThrowIfNull(professores);

        var dias = criterios.Dias.Distinct(StringComparer.Ordinal).ToList();
        var horariosCandidatos = GerarHorariosCandidatos(criterios);

        var encontrados = new List<ProfessorEncontrado>();
        var naoElegiveis = new List<ProfessorNaoElegivel>();

        foreach (var professor in professores)
        {
            // Regra 1 — precisa ensinar o idioma (primeiro motivo na ordem de prioridade).
            if (!EnsinaIdioma(professor, criterios.Idioma))
            {
                naoElegiveis.Add(new(professor.Id, professor.Nome, MotivoInelegibilidade.NaoEnsinaOIdioma));
                continue;
            }

            // Regra 2 — agenda bloqueada não recebe novos alunos.
            if (professor.AgendaBloqueada)
            {
                naoElegiveis.Add(new(professor.Id, professor.Nome, MotivoInelegibilidade.AgendaBloqueada));
                continue;
            }

            // Regras 3 a 6 — horário fixo livre em todos os dias pedidos.
            var horariosLivres = horariosCandidatos
                .Where(inicio => dias.All(dia => EstaLivre(professor, dia, inicio, criterios.DuracaoMinutos)))
                .Select(h => h.ToString())
                .ToList();

            if (horariosLivres.Count == 0)
            {
                naoElegiveis.Add(new(professor.Id, professor.Nome, MotivoInelegibilidade.SemHorarioEmComum));
                continue;
            }

            encontrados.Add(new(professor.Id, professor.Nome, professor.AulasNaSemana, horariosLivres));
        }

        // Regra 7 — menos aulas na semana primeiro; empate por nome (alfabético, sem distinguir caixa/acento).
        var ordenados = encontrados
            .OrderBy(p => p.AulasNaSemana)
            .ThenBy(p => p.Nome, StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase: true))
            .ToList();

        return new ResultadoDaBusca(ordenados, naoElegiveis);
    }

    /// <summary>
    /// Regras 4 e 6 pelo lado do aluno: inícios em :00/:30, a partir da faixa pedida,
    /// desde que a aula termine dentro da faixa. Já vêm em ordem crescente.
    /// </summary>
    internal static IReadOnlyList<Horario> GerarHorariosCandidatos(CriteriosDeBusca criterios)
    {
        var candidatos = new List<Horario>();
        var inicio = criterios.InicioFaixa.ArredondarParaMeiaHoraSeguinte();

        while (inicio.Somar(criterios.DuracaoMinutos) <= criterios.FimFaixa)
        {
            candidatos.Add(inicio);
            inicio = inicio.Somar(PassoEmMinutos);
        }

        return candidatos;
    }

    private static bool EnsinaIdioma(Professor professor, string idioma) =>
        professor.Idiomas.Any(i => Comparador.Compare(i.Trim(), idioma.Trim(), OpcoesIdioma) == 0);

    /// <summary>
    /// Em um dia específico, a aula [inicio, inicio+duração) cabe inteira em algum bloco
    /// de disponibilidade (regra 4) e não se sobrepõe a nenhuma aula marcada (regra 5).
    /// </summary>
    private static bool EstaLivre(Professor professor, string dia, Horario inicio, int duracaoMinutos)
    {
        var fim = inicio.Somar(duracaoMinutos);

        var cabeEmAlgumBloco = professor.Disponibilidade
            .Any(b => DiaIgual(b.Dia, dia) && b.Contem(inicio, fim));

        if (!cabeEmAlgumBloco) return false;

        var conflita = professor.AulasMarcadas
            .Any(a => DiaIgual(a.Dia, dia) && a.SobrepoeCom(inicio, fim));

        return !conflita;
    }

    private static bool DiaIgual(string a, string b) =>
        string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
}
