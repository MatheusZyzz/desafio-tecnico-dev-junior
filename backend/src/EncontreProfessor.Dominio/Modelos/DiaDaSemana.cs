namespace EncontreProfessor.Dominio.Modelos;

/// <summary>
/// Dias da semana exatamente como o desafio define (sem acento, minúsculos).
/// </summary>
public static class DiaDaSemana
{
    public static readonly IReadOnlyList<string> Todos = new[]
    {
        "segunda", "terca", "quarta", "quinta", "sexta", "sabado", "domingo"
    };

    private static readonly HashSet<string> Conjunto = new(Todos, StringComparer.Ordinal);

    /// <summary>
    /// Normaliza a entrada (trim + minúsculas) e devolve o dia canônico, ou null se for inválido.
    /// Decisão: aceitamos "Segunda" ou " segunda ", mas não "terça" com acento — o contrato
    /// do desafio é explícito quanto à grafia sem acento.
    /// </summary>
    public static string? Normalizar(string? dia)
    {
        if (string.IsNullOrWhiteSpace(dia)) return null;
        var normalizado = dia.Trim().ToLowerInvariant();
        return Conjunto.Contains(normalizado) ? normalizado : null;
    }

    public static bool EhValido(string? dia) => Normalizar(dia) is not null;
}
