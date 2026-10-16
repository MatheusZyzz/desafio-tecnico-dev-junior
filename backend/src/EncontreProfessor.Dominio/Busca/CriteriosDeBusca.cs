using EncontreProfessor.Dominio.Modelos;

namespace EncontreProfessor.Dominio.Busca;

/// <summary>
/// Critérios já validados e tipados. A camada HTTP converte a requisição crua
/// (strings) neste objeto por meio do <see cref="Validacao.ValidadorDeBusca"/>.
/// </summary>
public sealed record CriteriosDeBusca(
    string Idioma,
    IReadOnlyList<string> Dias,
    Horario InicioFaixa,
    Horario FimFaixa,
    int DuracaoMinutos)
{
    public static readonly IReadOnlyList<int> DuracoesPermitidas = new[] { 30, 60, 90 };
}
