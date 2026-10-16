namespace EncontreProfessor.Dominio.Modelos;

/// <summary>Bloco semanal em que o professor pode dar aula.</summary>
public sealed record BlocoDisponibilidade(string Dia, Horario Inicio, Horario Fim)
{
    /// <summary>A aula [inicio, fim) cabe inteira dentro do bloco (regra 4).</summary>
    public bool Contem(Horario inicioAula, Horario fimAula) => Inicio <= inicioAula && fimAula <= Fim;
}

/// <summary>Aula que o professor já tem marcada na semana.</summary>
public sealed record AulaMarcada(string Dia, Horario Inicio, int DuracaoMinutos)
{
    public Horario Fim => Inicio.Somar(DuracaoMinutos);

    /// <summary>
    /// Sobreposição estrita de intervalos semiabertos [inicio, fim).
    /// Encostar é permitido (regra 5): 18:00-19:00 e 19:00-20:00 não se sobrepõem.
    /// </summary>
    public bool SobrepoeCom(Horario inicioAula, Horario fimAula) => Inicio < fimAula && inicioAula < Fim;
}

public sealed record Professor(
    int Id,
    string Nome,
    IReadOnlyList<string> Idiomas,
    bool AgendaBloqueada,
    IReadOnlyList<BlocoDisponibilidade> Disponibilidade,
    IReadOnlyList<AulaMarcada> AulasMarcadas)
{
    public int AulasNaSemana => AulasMarcadas.Count;
}
