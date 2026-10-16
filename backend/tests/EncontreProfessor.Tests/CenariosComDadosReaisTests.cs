using EncontreProfessor.Api.Dados;
using EncontreProfessor.Dominio.Busca;
using EncontreProfessor.Dominio.Modelos;
using EncontreProfessor.Tests.Fixtures;

namespace EncontreProfessor.Tests;

/// <summary>
/// Executa a busca sobre o professores.json real (copiado para a saída de testes),
/// sem subir a API. O primeiro caso é exatamente o exemplo do enunciado.
/// </summary>
public class CenariosComDadosReaisTests
{
    private static readonly IReadOnlyList<Professor> Dados =
        new RepositorioDeProfessoresJson(Path.Combine(AppContext.BaseDirectory, "professores.json")).ObterTodos();

    private readonly BuscaDeProfessores _busca = new();

    [Fact]
    public void ArquivoCarrega_OsOitoProfessores()
    {
        Assert.Equal(8, Dados.Count);
        Assert.Contains(Dados, p => p.Nome == "Fábio" && p.Disponibilidade.Any(b => b.Inicio.ToString() == "18:15"));
    }

    [Fact]
    public void ExemploDoEnunciado_EspanholSegunda18as20_60min()
    {
        var criterios = Professores.Criterios(idioma: "Espanhol", dias: new[] { "segunda" }, inicio: "18:00", fim: "20:00", duracao: 60);

        var resultado = _busca.Buscar(criterios, Dados);

        Assert.Equal(2, resultado.Professores.Count);

        Assert.Equal(1, resultado.Professores[0].Id);
        Assert.Equal("Ana", resultado.Professores[0].Nome);
        Assert.Equal(1, resultado.Professores[0].AulasNaSemana);
        Assert.Equal(new[] { "18:00" }, resultado.Professores[0].Horarios);

        Assert.Equal(4, resultado.Professores[1].Id);
        Assert.Equal("Diego", resultado.Professores[1].Nome);
        Assert.Equal(1, resultado.Professores[1].AulasNaSemana);
        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, resultado.Professores[1].Horarios);

        Assert.Equal(6, resultado.NaoElegiveis.Count);
        Assert.All(resultado.NaoElegiveis, n => Assert.Equal(MotivoInelegibilidade.NaoEnsinaOIdioma, n.Motivo));
        Assert.Contains(resultado.NaoElegiveis, n => n.Id == 2 && n.Nome == "Bruno");
    }

    [Fact]
    public void InglesSegundaEQuarta_18as21_60min()
    {
        var criterios = Professores.Criterios(idioma: "Inglês", dias: new[] { "segunda", "quarta" }, inicio: "18:00", fim: "21:00", duracao: 60);

        var resultado = _busca.Buscar(criterios, Dados);

        Assert.Equal(new[] { "Ana", "Fábio", "Bruno", "Gabriela" }, resultado.Professores.Select(p => p.Nome));
        Assert.Equal(new[] { "18:00", "20:00" }, resultado.Professores.Single(p => p.Nome == "Ana").Horarios);
        Assert.Equal(new[] { "18:30", "19:00" }, resultado.Professores.Single(p => p.Nome == "Fábio").Horarios);
        Assert.Equal(new[] { "18:00", "20:00" }, resultado.Professores.Single(p => p.Nome == "Bruno").Horarios);
        Assert.Equal(new[] { "18:30", "19:00", "19:30" }, resultado.Professores.Single(p => p.Nome == "Gabriela").Horarios);

        var motivos = resultado.NaoElegiveis.ToDictionary(n => n.Nome, n => n.Motivo);
        Assert.Equal(MotivoInelegibilidade.AgendaBloqueada, motivos["Carla"]);
        Assert.Equal(MotivoInelegibilidade.NaoEnsinaOIdioma, motivos["Diego"]);
        Assert.Equal(MotivoInelegibilidade.SemHorarioEmComum, motivos["Elisa"]);
        Assert.Equal(MotivoInelegibilidade.SemHorarioEmComum, motivos["Heitor"]);
    }

    [Fact]
    public void InglesQuarta_18as22_90min()
    {
        var criterios = Professores.Criterios(idioma: "Inglês", dias: new[] { "quarta" }, inicio: "18:00", fim: "22:00", duracao: 90);

        var resultado = _busca.Buscar(criterios, Dados);

        Assert.Equal(new[] { "Ana", "Fábio", "Bruno", "Gabriela" }, resultado.Professores.Select(p => p.Nome));
        Assert.Equal(new[] { "18:00", "18:30", "19:00", "19:30", "20:00", "20:30" }, resultado.Professores.Single(p => p.Nome == "Ana").Horarios);
        Assert.Equal(new[] { "18:30" }, resultado.Professores.Single(p => p.Nome == "Fábio").Horarios);
        Assert.Equal(new[] { "20:00", "20:30" }, resultado.Professores.Single(p => p.Nome == "Bruno").Horarios);
        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, resultado.Professores.Single(p => p.Nome == "Gabriela").Horarios);
    }
}
