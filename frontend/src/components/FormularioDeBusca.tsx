import { useState, type FormEvent } from 'react'
import { DIAS, DURACOES, IDIOMAS, type BuscaRequest } from '../api/tipos'

interface Props {
  carregando: boolean
  onBuscar: (request: BuscaRequest) => void
}

const VALORES_INICIAIS: BuscaRequest = {
  idioma: 'Espanhol',
  dias: ['segunda'],
  inicioFaixa: '18:00',
  fimFaixa: '20:00',
  duracaoMinutos: 60,
}

/**
 * Formulário controlado. Decisão: o front NÃO replica a validação da API (campos obrigatórios,
 * formato de horário, ordem da faixa). Ele envia o que o usuário preencheu e exibe os erros 400
 * devolvidos pela API — era um dos estados exigidos e assim a regra fica em um lugar só.
 */
export function FormularioDeBusca({ carregando, onBuscar }: Props) {
  const [valores, setValores] = useState<BuscaRequest>(VALORES_INICIAIS)

  function alternarDia(dia: string) {
    setValores((v) => ({
      ...v,
      dias: v.dias.includes(dia) ? v.dias.filter((d) => d !== dia) : [...v.dias, dia],
    }))
  }

  function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault()
    onBuscar(valores)
  }

  return (
    <form className="formulario" onSubmit={enviar} noValidate aria-busy={carregando}>
      <div className="campo">
        <label htmlFor="idioma">Idioma</label>
        <select
          id="idioma"
          value={valores.idioma}
          onChange={(e) => setValores({ ...valores, idioma: e.target.value })}
        >
          {IDIOMAS.map((idioma) => (
            <option key={idioma} value={idioma}>
              {idioma}
            </option>
          ))}
        </select>
      </div>

      <fieldset className="campo">
        <legend>Dias da semana</legend>
        <div className="dias">
          {DIAS.map(({ valor, rotulo }) => (
            <label key={valor} className={`dia ${valores.dias.includes(valor) ? 'dia--ativo' : ''}`}>
              <input
                type="checkbox"
                checked={valores.dias.includes(valor)}
                onChange={() => alternarDia(valor)}
              />
              {rotulo}
            </label>
          ))}
        </div>
      </fieldset>

      <div className="linha">
        <div className="campo">
          <label htmlFor="inicioFaixa">Das</label>
          <input
            id="inicioFaixa"
            type="time"
            step={1800}
            value={valores.inicioFaixa}
            onChange={(e) => setValores({ ...valores, inicioFaixa: e.target.value })}
          />
        </div>
        <div className="campo">
          <label htmlFor="fimFaixa">Até</label>
          <input
            id="fimFaixa"
            type="time"
            step={1800}
            value={valores.fimFaixa}
            onChange={(e) => setValores({ ...valores, fimFaixa: e.target.value })}
          />
        </div>
        <div className="campo">
          <label htmlFor="duracao">Duração</label>
          <select
            id="duracao"
            value={valores.duracaoMinutos}
            onChange={(e) => setValores({ ...valores, duracaoMinutos: Number(e.target.value) })}
          >
            {DURACOES.map((d) => (
              <option key={d} value={d}>
                {d} min
              </option>
            ))}
          </select>
        </div>
      </div>

      <button type="submit" className="botao" disabled={carregando}>
        {carregando ? 'Buscando…' : 'Buscar professores'}
      </button>
    </form>
  )
}
