import type { BuscaResponse } from '../api/tipos'

/** Estados possíveis da área de resultado — um por exigência da seção 4 do desafio. */
export type EstadoDaBusca =
  | { tipo: 'inicial' }
  | { tipo: 'carregando' }
  | { tipo: 'sucesso'; dados: BuscaResponse }
  | { tipo: 'erro-validacao'; mensagem: string; erros: string[] }
  | { tipo: 'erro-comunicacao'; mensagem: string }

interface Props {
  estado: EstadoDaBusca
  onTentarNovamente?: () => void
}

export function ResultadoDaBusca({ estado, onTentarNovamente }: Props) {
  switch (estado.tipo) {
    case 'inicial':
      return (
        <p className="aviso aviso--neutro">
          Preencha o formulário e clique em <strong>Buscar professores</strong>.
        </p>
      )

    case 'carregando':
      return (
        <div className="aviso aviso--neutro" role="status" aria-live="polite">
          <span className="spinner" aria-hidden="true" /> Procurando professores disponíveis…
        </div>
      )

    case 'erro-validacao':
      return (
        <div className="aviso aviso--erro" role="alert">
          <strong>{estado.mensagem}</strong>
          <ul>
            {estado.erros.map((erro) => (
              <li key={erro}>{erro}</li>
            ))}
          </ul>
        </div>
      )

    case 'erro-comunicacao':
      return (
        <div className="aviso aviso--erro" role="alert">
          <strong>Falha de comunicação com a API.</strong>
          <p>{estado.mensagem}</p>
          {onTentarNovamente && (
            <button type="button" className="botao botao--secundario" onClick={onTentarNovamente}>
              Tentar novamente
            </button>
          )}
        </div>
      )

    case 'sucesso':
      return <Sucesso dados={estado.dados} />
  }
}

function Sucesso({ dados }: { dados: BuscaResponse }) {
  const { professores, naoElegiveis } = dados

  return (
    <div className="resultado">
      <section aria-labelledby="titulo-encontrados">
        <h2 id="titulo-encontrados">
          Professores disponíveis <span className="contador">{professores.length}</span>
        </h2>

        {professores.length === 0 ? (
          <p className="aviso aviso--vazio">
            Nenhum professor encontrado para esses critérios. Tente outros dias, outra faixa de
            horário ou uma aula mais curta.
          </p>
        ) : (
          <ul className="cartoes">
            {professores.map((p) => (
              <li key={p.id} className="cartao">
                <div className="cartao__cabecalho">
                  <h3>{p.nome}</h3>
                  <span className="etiqueta">
                    {p.aulasNaSemana} {p.aulasNaSemana === 1 ? 'aula' : 'aulas'} na semana
                  </span>
                </div>
                <p className="cartao__rotulo">Horários possíveis</p>
                <ul className="horarios">
                  {p.horarios.map((h) => (
                    <li key={h} className="horario">
                      {h}
                    </li>
                  ))}
                </ul>
              </li>
            ))}
          </ul>
        )}
      </section>

      {naoElegiveis.length > 0 && (
        <details className="nao-elegiveis">
          <summary>
            Professores que ficaram de fora <span className="contador">{naoElegiveis.length}</span>
          </summary>
          <table>
            <thead>
              <tr>
                <th scope="col">Professor</th>
                <th scope="col">Motivo</th>
              </tr>
            </thead>
            <tbody>
              {naoElegiveis.map((n) => (
                <tr key={n.id}>
                  <td>{n.nome}</td>
                  <td>{n.motivo}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </details>
      )}
    </div>
  )
}
