using EncontreProfessor.Dominio.Busca;
using EncontreProfessor.Tests.Fixtures;
using static EncontreProfessor.Tests.Fixtures.Professores;

namespace EncontreProfessor.Tests;

/// <summary>
/// Um teste por regra da seção 2 do desafio, com foco nos casos de fronteira.
/// Os cenários são montados em memória; nada aqui depende de subir a API.
/// </summary>
public class BuscaDeProfessoresTests
{
    private readonly BuscaDeProfessores _busca = new();

    // Regra 1 — precisa ensinar o idioma
    [Fact]
    public void Regra1_ProfessorQueNaoEnsinaOIdioma_FicaDeForaComMotivo()
    {
        var professores = new[]
        {
            Novo(1, "Ana", idiomas: new[] { "Espanhol" }, disponibilidade: new[] { ("segunda", "18:00", "22:00") }),
            Novo(2, "Bruno", idiomas: new[] { "Inglês" }, disponibilidade: new[] { ("segunda", "18:00", "22:00") }),
        };

        var resultado = _busca.Buscar(Criterios(idioma: "Espanhol"), professores);

        Assert.Single(resultado.Professores);
        Assert.Equal("Ana", resultado.Professores[0].Nome);

        var bruno = Assert.Single(resultado.NaoElegiveis);
        Assert.Equal(MotivoInelegibilidade.NaoEnsinaOIdioma, bruno.Motivo);
    }

    [Fact]
    public void Regra1_ComparacaoDeIdiomaIgnoraCaixaEAcento()
    {
        var professores = new[]
        {
            Novo(1, "Ana", idiomas: new[] { "Inglês" }, disponibilidade: new[] { ("segunda", "18:00", "22:00") }),
        };

        var resultado = _busca.Buscar(Criterios(idioma: "ingles"), professores);

        Assert.Single(resultado.Professores);
        Assert.Empty(resultado.NaoElegiveis);
    }

    // Regra 2 — agenda bloqueada
    [Fact]
    public void Regra2_ProfessorComAgendaBloqueada_NaoAparece()
    {
        var professores = new[]
        {
            Novo(3, "Carla", bloqueado: true, disponibilidade: new[] { ("segunda", "08:00", "22:00") }),
        };

        var resultado = _busca.Buscar(Criterios(), professores);

        Assert.Empty(resultado.Professores);
        var carla = Assert.Single(resultado.NaoElegiveis);
        Assert.Equal(MotivoInelegibilidade.AgendaBloqueada, carla.Motivo);
    }

    // Regra 3 — horário fixo em todos os dias pedidos
    [Fact]
    public void Regra3_HorarioFixo_SoVale_SeEstiverLivreEmTodosOsDias()
    {
        // Segunda: livre 18:00 e 20:00 (aula 19-20). Quarta: livre só 18:00 (aula 19-21).
        var professores = new[]
        {
            Novo(1, "Ana",
                disponibilidade: new[] { ("segunda", "18:00", "21:00"), ("quarta", "18:00", "21:00") },
                aulas: new[] { ("segunda", "19:00", 60), ("quarta", "19:00", 120) }),
        };

        var resultado = _busca.Buscar(Criterios(dias: new[] { "segunda", "quarta" }, fim: "21:00"), professores);

        var ana = Assert.Single(resultado.Professores);
        Assert.Equal(new[] { "18:00" }, ana.Horarios);
    }

    [Fact]
    public void Regra3_SemHorarioComumEntreOsDias_FicaDeForaComMotivo()
    {
        // Segunda só 18:00 livre; quarta só 20:00 livre → interseção vazia.
        var professores = new[]
        {
            Novo(8, "Heitor",
                disponibilidade: new[] { ("segunda", "18:00", "21:00"), ("quarta", "18:00", "21:00") },
                aulas: new[] { ("segunda", "19:00", 60), ("segunda", "20:00", 60), ("quarta", "18:00", 60), ("quarta", "19:00", 60) }),
        };

        var resultado = _busca.Buscar(Criterios(dias: new[] { "segunda", "quarta" }, fim: "21:00"), professores);

        Assert.Empty(resultado.Professores);
        Assert.Equal(MotivoInelegibilidade.SemHorarioEmComum, Assert.Single(resultado.NaoElegiveis).Motivo);
    }

    // Regra 4 — a aula cabe inteira no bloco e na faixa
    [Fact]
    public void Regra4_AulaPrecisaCaberInteiraNoBlocoDeDisponibilidade()
    {
        // Bloco 18:15–20:00: 18:00 não cabe (começa antes), 19:30 não cabe (termina 20:30).
        var professores = new[]
        {
            Novo(6, "Fábio", disponibilidade: new[] { ("segunda", "18:15", "20:00") }),
        };

        var resultado = _busca.Buscar(Criterios(inicio: "18:00", fim: "21:00", duracao: 60), professores);

        var fabio = Assert.Single(resultado.Professores);
        Assert.Equal(new[] { "18:30", "19:00" }, fabio.Horarios);
    }

    [Fact]
    public void Regra4_AulaPrecisaCaberInteiraNaFaixaPedidaPeloAluno()
    {
        // Professor livre 18–22, mas aluno só pode até 19:30 → 60 min cabe só às 18:00 e 18:30.
        var professores = new[]
        {
            Novo(5, "Elisa", disponibilidade: new[] { ("segunda", "18:00", "22:00") }),
        };

        var resultado = _busca.Buscar(Criterios(inicio: "18:00", fim: "19:30", duracao: 60), professores);

        Assert.Equal(new[] { "18:00", "18:30" }, Assert.Single(resultado.Professores).Horarios);
    }

    [Fact]
    public void Regra4_AulaNaoPodeSeEspalharPorDoisBlocosSeparados()
    {
        // O dado real garante que blocos não se encostam; aqui forçamos blocos adjacentes para
        // provar que a aula não pode "atravessar" a fronteira: 18:30–19:30 não cabe em nenhum deles.
        var professores = new[]
        {
            Novo(2, "Bruno", disponibilidade: new[] { ("quarta", "18:00", "19:00"), ("quarta", "19:00", "21:00") }),
        };

        var resultado = _busca.Buscar(Criterios(dias: new[] { "quarta" }, inicio: "18:00", fim: "21:00", duracao: 60), professores);

        Assert.Equal(new[] { "18:00", "19:00", "19:30", "20:00" }, Assert.Single(resultado.Professores).Horarios);
    }

    // Regra 5 — sem sobreposição; encostar é permitido
    [Fact]
    public void Regra5_EncostarEmAulaMarcadaEhPermitido_SobreporNao()
    {
        // Aula marcada 19:00–20:00. Faixa 18–21, 60 min:
        // 18:00 termina 19:00 (encosta: ok), 18:30 sobrepõe, 19:00 sobrepõe, 19:30 sobrepõe, 20:00 começa quando termina (ok).
        var professores = new[]
        {
            Novo(1, "Ana", disponibilidade: new[] { ("segunda", "18:00", "22:00") }, aulas: new[] { ("segunda", "19:00", 60) }),
        };

        var resultado = _busca.Buscar(Criterios(inicio: "18:00", fim: "21:00", duracao: 60), professores);

        Assert.Equal(new[] { "18:00", "20:00" }, Assert.Single(resultado.Professores).Horarios);
    }

    [Fact]
    public void Regra5_AulaMarcadaEmOutroDiaNaoInterfere()
    {
        var professores = new[]
        {
            Novo(4, "Diego", disponibilidade: new[] { ("segunda", "18:00", "20:00") }, aulas: new[] { ("terca", "18:00", 120) }),
        };

        var resultado = _busca.Buscar(Criterios(), professores);

        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, Assert.Single(resultado.Professores).Horarios);
    }

    // Regra 6 — só :00 ou :30
    [Fact]
    public void Regra6_InicioDaFaixaForaDaMeiaHora_ComecaNoProximoSlotValido()
    {
        var professores = new[]
        {
            Novo(5, "Elisa", disponibilidade: new[] { ("segunda", "18:00", "22:00") }),
        };

        var resultado = _busca.Buscar(Criterios(inicio: "18:15", fim: "20:00", duracao: 30), professores);

        var horarios = Assert.Single(resultado.Professores).Horarios;
        Assert.Equal(new[] { "18:30", "19:00", "19:30" }, horarios);
        Assert.All(horarios, h => Assert.True(h.EndsWith(":00") || h.EndsWith(":30")));
    }

    // Regra 7 — ordenação
    [Fact]
    public void Regra7_OrdenaPorMenosAulasNaSemana_DepoisPorNome()
    {
        var disp = new[] { ("segunda", "18:00", "22:00") };
        var professores = new[]
        {
            Novo(1, "Zeca", disponibilidade: disp, aulas: new[] { ("sexta", "09:00", 60) }),
            Novo(2, "Bia", disponibilidade: disp, aulas: new[] { ("sexta", "09:00", 60), ("sexta", "10:00", 60) }),
            Novo(3, "Caio", disponibilidade: disp),
            Novo(4, "Ana", disponibilidade: disp, aulas: new[] { ("sexta", "11:00", 60) }),
        };

        var resultado = _busca.Buscar(Criterios(), professores);

        Assert.Equal(new[] { "Caio", "Ana", "Zeca", "Bia" }, resultado.Professores.Select(p => p.Nome));
        Assert.Equal(new[] { 0, 1, 1, 2 }, resultado.Professores.Select(p => p.AulasNaSemana));
    }

    [Fact]
    public void Regra7_HorariosDeCadaProfessorVemEmOrdemCrescente()
    {
        var professores = new[]
        {
            Novo(1, "Ana", disponibilidade: new[] { ("segunda", "08:00", "12:00") }),
        };

        var resultado = _busca.Buscar(Criterios(inicio: "08:00", fim: "12:00", duracao: 90), professores);

        var horarios = Assert.Single(resultado.Professores).Horarios;
        Assert.Equal(new[] { "08:00", "08:30", "09:00", "09:30", "10:00", "10:30" }, horarios);
        Assert.Equal(horarios.OrderBy(h => h).ToList(), horarios.ToList());
    }

    // Diferencial — prioridade dos motivos
    [Fact]
    public void NaoElegiveis_QuandoHaMaisDeUmMotivo_InformaOPrimeiroNaOrdem()
    {
        var professores = new[]
        {
            // Não ensina E está bloqueado → "Não ensina o idioma"
            Novo(1, "Ana", idiomas: new[] { "Francês" }, bloqueado: true),
            // Bloqueado E sem horário (nenhuma disponibilidade) → "Agenda bloqueada"
            Novo(2, "Bruno", bloqueado: true),
        };

        var resultado = _busca.Buscar(Criterios(), professores);

        Assert.Empty(resultado.Professores);
        Assert.Equal(MotivoInelegibilidade.NaoEnsinaOIdioma, resultado.NaoElegiveis.Single(n => n.Nome == "Ana").Motivo);
        Assert.Equal(MotivoInelegibilidade.AgendaBloqueada, resultado.NaoElegiveis.Single(n => n.Nome == "Bruno").Motivo);
    }

    // Decisão própria — dias repetidos na requisição
    [Fact]
    public void DiasRepetidosNaRequisicao_SaoTratadosComoUmSo()
    {
        var professores = new[]
        {
            Novo(1, "Ana", disponibilidade: new[] { ("segunda", "18:00", "20:00") }),
        };

        var resultado = _busca.Buscar(Criterios(dias: new[] { "segunda", "segunda" }), professores);

        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, Assert.Single(resultado.Professores).Horarios);
    }

    // Decisão própria — faixa mais curta que a duração não é erro de validação, só não gera horários
    [Fact]
    public void FaixaMenorQueADuracao_NaoGeraHorarios_EMarcaComoSemHorarioEmComum()
    {
        var professores = new[]
        {
            Novo(1, "Ana", disponibilidade: new[] { ("segunda", "08:00", "22:00") }),
        };

        var resultado = _busca.Buscar(Criterios(inicio: "18:00", fim: "18:30", duracao: 60), professores);

        Assert.Empty(resultado.Professores);
        Assert.Equal(MotivoInelegibilidade.SemHorarioEmComum, Assert.Single(resultado.NaoElegiveis).Motivo);
    }
}
