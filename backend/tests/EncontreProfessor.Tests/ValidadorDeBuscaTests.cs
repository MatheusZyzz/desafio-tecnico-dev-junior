using EncontreProfessor.Dominio.Validacao;

namespace EncontreProfessor.Tests;

/// <summary>Seção 3 do desafio: cada condição de 400 produz uma mensagem clara.</summary>
public class ValidadorDeBuscaTests
{
    private static RequisicaoDeBusca Valida() =>
        new("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60);

    [Fact]
    public void RequisicaoValida_ProduzCriteriosTipados()
    {
        var resultado = ValidadorDeBusca.Validar(Valida());

        Assert.True(resultado.EhValido);
        Assert.Empty(resultado.Erros);
        Assert.NotNull(resultado.Criterios);
        Assert.Equal("Espanhol", resultado.Criterios!.Idioma);
        Assert.Equal("18:00", resultado.Criterios.InicioFaixa.ToString());
        Assert.Equal(60, resultado.Criterios.DuracaoMinutos);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IdiomaVazio_EhInvalido(string? idioma)
    {
        var resultado = ValidadorDeBusca.Validar(Valida() with { Idioma = idioma });

        Assert.False(resultado.EhValido);
        Assert.Contains(resultado.Erros, e => e.Contains("idioma", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void NenhumDiaInformado_EhInvalido()
    {
        Assert.False(ValidadorDeBusca.Validar(Valida() with { Dias = null }).EhValido);
        Assert.False(ValidadorDeBusca.Validar(Valida() with { Dias = Array.Empty<string?>() }).EhValido);
    }

    [Theory]
    [InlineData("segunda-feira")]
    [InlineData("terça")]
    [InlineData("monday")]
    [InlineData("")]
    public void DiaInvalido_EhRejeitadoComMensagemQueCitaODia(string dia)
    {
        var resultado = ValidadorDeBusca.Validar(Valida() with { Dias = new[] { "segunda", dia } });

        Assert.False(resultado.EhValido);
        Assert.Contains(resultado.Erros, e => e.StartsWith("Dia inválido", StringComparison.Ordinal));
    }

    [Fact]
    public void DiaComCaixaDiferenteOuEspacos_EhAceitoENormalizado()
    {
        var resultado = ValidadorDeBusca.Validar(Valida() with { Dias = new[] { " Segunda ", "QUARTA" } });

        Assert.True(resultado.EhValido);
        Assert.Equal(new[] { "segunda", "quarta" }, resultado.Criterios!.Dias);
    }

    [Theory]
    [InlineData("18h")]
    [InlineData("18:0")]
    [InlineData("25:00")]
    [InlineData("18:60")]
    [InlineData("6:00")]
    [InlineData("")]
    [InlineData(null)]
    public void HorarioForaDoFormatoHHmm_EhInvalido(string? horario)
    {
        var comInicioRuim = ValidadorDeBusca.Validar(Valida() with { InicioFaixa = horario });
        var comFimRuim = ValidadorDeBusca.Validar(Valida() with { FimFaixa = horario });

        Assert.False(comInicioRuim.EhValido);
        Assert.False(comFimRuim.EhValido);
        Assert.Contains(comInicioRuim.Erros, e => e.Contains("HH:mm"));
        Assert.Contains(comFimRuim.Erros, e => e.Contains("HH:mm"));
    }

    [Theory]
    [InlineData("18:00", "18:00")]
    [InlineData("20:00", "18:00")]
    public void FimDaFaixaIgualOuAnteriorAoInicio_EhInvalido(string inicio, string fim)
    {
        var resultado = ValidadorDeBusca.Validar(Valida() with { InicioFaixa = inicio, FimFaixa = fim });

        Assert.False(resultado.EhValido);
        Assert.Contains("O fim da faixa deve ser posterior ao início.", resultado.Erros);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(45)]
    [InlineData(120)]
    [InlineData(-30)]
    [InlineData(null)]
    public void DuracaoDiferenteDe30_60_90_EhInvalida(int? duracao)
    {
        var resultado = ValidadorDeBusca.Validar(Valida() with { DuracaoMinutos = duracao });

        Assert.False(resultado.EhValido);
        Assert.Contains("A duração deve ser 30, 60 ou 90 minutos.", resultado.Erros);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(90)]
    public void DuracoesPermitidas_SaoAceitas(int duracao)
    {
        Assert.True(ValidadorDeBusca.Validar(Valida() with { DuracaoMinutos = duracao }).EhValido);
    }

    [Fact]
    public void VariosProblemas_SaoReportadosJuntos()
    {
        var resultado = ValidadorDeBusca.Validar(new RequisicaoDeBusca("", null, "x", "y", 10));

        Assert.False(resultado.EhValido);
        Assert.True(resultado.Erros.Count >= 4, $"Esperava ao menos 4 erros, obteve {resultado.Erros.Count}.");
    }

    [Fact]
    public void CorpoNulo_EhInvalido()
    {
        var resultado = ValidadorDeBusca.Validar(null);

        Assert.False(resultado.EhValido);
        Assert.Single(resultado.Erros);
    }
}
