namespace EncontreProfessor.Dominio.Busca;

public sealed record ProfessorEncontrado(
    int Id,
    string Nome,
    int AulasNaSemana,
    IReadOnlyList<string> Horarios);

public sealed record ProfessorNaoElegivel(
    int Id,
    string Nome,
    string Motivo);

public sealed record ResultadoDaBusca(
    IReadOnlyList<ProfessorEncontrado> Professores,
    IReadOnlyList<ProfessorNaoElegivel> NaoElegiveis);

/// <summary>
/// Textos exatos exigidos pelo desafio. A ordem das constantes é a ordem de prioridade
/// quando mais de um motivo se aplica ao mesmo professor.
/// </summary>
public static class MotivoInelegibilidade
{
    public const string NaoEnsinaOIdioma = "Não ensina o idioma";
    public const string AgendaBloqueada = "Agenda bloqueada";
    public const string SemHorarioEmComum = "Sem horário em comum nos dias pedidos";
}
