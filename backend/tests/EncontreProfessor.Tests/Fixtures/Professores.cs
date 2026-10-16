using EncontreProfessor.Dominio.Busca;
using EncontreProfessor.Dominio.Modelos;

namespace EncontreProfessor.Tests.Fixtures;

/// <summary>Construtores compactos para montar cenários nos testes sem depender de arquivo ou API.</summary>
internal static class Professores
{
    public static Professor Novo(
        int id,
        string nome,
        string[]? idiomas = null,
        bool bloqueado = false,
        (string dia, string inicio, string fim)[]? disponibilidade = null,
        (string dia, string inicio, int duracao)[]? aulas = null)
    {
        return new Professor(
            id,
            nome,
            idiomas ?? new[] { "Inglês" },
            bloqueado,
            (disponibilidade ?? Array.Empty<(string, string, string)>())
                .Select(d => new BlocoDisponibilidade(d.dia, Horario.Parse(d.inicio), Horario.Parse(d.fim)))
                .ToList(),
            (aulas ?? Array.Empty<(string, string, int)>())
                .Select(a => new AulaMarcada(a.dia, Horario.Parse(a.inicio), a.duracao))
                .ToList());
    }

    public static CriteriosDeBusca Criterios(
        string idioma = "Inglês",
        string[]? dias = null,
        string inicio = "18:00",
        string fim = "20:00",
        int duracao = 60)
    {
        return new CriteriosDeBusca(idioma, dias ?? new[] { "segunda" }, Horario.Parse(inicio), Horario.Parse(fim), duracao);
    }
}
