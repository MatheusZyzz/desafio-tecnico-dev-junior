using System.Globalization;
using System.Text.RegularExpressions;

namespace EncontreProfessor.Dominio.Modelos;

/// <summary>
/// Horário do dia representado em minutos desde a meia-noite (0..1439).
/// Evita aritmética com strings e deixa as comparações de sobreposição triviais.
/// </summary>
public readonly record struct Horario(int Minutos) : IComparable<Horario>
{
    private static readonly Regex FormatoHHmm = new(@"^([01]\d|2[0-3]):([0-5]\d)$", RegexOptions.Compiled);

    public const int MinutosNoDia = 24 * 60;

    public int Horas => Minutos / 60;
    public int MinutosDaHora => Minutos % 60;

    /// <summary>A aula só pode começar em hora cheia ou meia hora (regra 6).</summary>
    public bool EstaNaMeiaHora => Minutos % 30 == 0;

    public static bool TryParse(string? texto, out Horario horario)
    {
        horario = default;
        if (string.IsNullOrWhiteSpace(texto)) return false;

        var match = FormatoHHmm.Match(texto.Trim());
        if (!match.Success) return false;

        var horas = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var minutos = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        horario = new Horario(horas * 60 + minutos);
        return true;
    }

    public static Horario Parse(string texto)
    {
        if (!TryParse(texto, out var horario))
            throw new FormatException($"Horário inválido: '{texto}'. Esperado o formato HH:mm.");
        return horario;
    }

    public Horario Somar(int minutos) => new(Minutos + minutos);

    /// <summary>Arredonda para cima até o próximo múltiplo de 30 minutos (:00 ou :30).</summary>
    public Horario ArredondarParaMeiaHoraSeguinte()
    {
        var resto = Minutos % 30;
        return resto == 0 ? this : new Horario(Minutos + (30 - resto));
    }

    public int CompareTo(Horario outro) => Minutos.CompareTo(outro.Minutos);

    public static bool operator <(Horario a, Horario b) => a.Minutos < b.Minutos;
    public static bool operator >(Horario a, Horario b) => a.Minutos > b.Minutos;
    public static bool operator <=(Horario a, Horario b) => a.Minutos <= b.Minutos;
    public static bool operator >=(Horario a, Horario b) => a.Minutos >= b.Minutos;

    public override string ToString() => $"{Horas:00}:{MinutosDaHora:00}";
}
