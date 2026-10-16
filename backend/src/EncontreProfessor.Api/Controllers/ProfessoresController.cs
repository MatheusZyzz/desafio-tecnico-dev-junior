using EncontreProfessor.Api.Dados;
using EncontreProfessor.Dominio.Busca;
using EncontreProfessor.Dominio.Validacao;
using Microsoft.AspNetCore.Mvc;

namespace EncontreProfessor.Api.Controllers;

/// <summary>Corpo de POST /api/professores/busca. Todos os campos opcionais: a validação é do domínio.</summary>
public sealed class BuscaRequest
{
    public string? Idioma { get; set; }
    public List<string?>? Dias { get; set; }
    public string? InicioFaixa { get; set; }
    public string? FimFaixa { get; set; }
    public int? DuracaoMinutos { get; set; }
}

/// <summary>Formato único de erro 400 da API.</summary>
public sealed record ErroDeValidacaoResponse(string Mensagem, IReadOnlyList<string> Erros);

[ApiController]
[Route("api/professores")]
[Produces("application/json")]
public sealed class ProfessoresController : ControllerBase
{
    private readonly IRepositorioDeProfessores _repositorio;
    private readonly BuscaDeProfessores _busca;

    public ProfessoresController(IRepositorioDeProfessores repositorio, BuscaDeProfessores busca)
    {
        _repositorio = repositorio;
        _busca = busca;
    }

    [HttpPost("busca")]
    [ProducesResponseType(typeof(ResultadoDaBusca), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDeValidacaoResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<ResultadoDaBusca> Buscar([FromBody] BuscaRequest? request)
    {
        var validacao = ValidadorDeBusca.Validar(request is null
            ? null
            : new RequisicaoDeBusca(request.Idioma, request.Dias, request.InicioFaixa, request.FimFaixa, request.DuracaoMinutos));

        if (!validacao.EhValido)
            return BadRequest(new ErroDeValidacaoResponse("Requisição inválida.", validacao.Erros));

        var resultado = _busca.Buscar(validacao.Criterios!, _repositorio.ObterTodos());
        return Ok(resultado);
    }
}
