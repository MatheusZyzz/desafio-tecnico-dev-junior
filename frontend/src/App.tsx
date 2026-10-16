import { useCallback, useEffect, useRef, useState } from 'react'
import { buscarProfessores, ErroDeComunicacao, ErroDeValidacao } from './api/professores'
import type { BuscaRequest } from './api/tipos'
import { FormularioDeBusca } from './components/FormularioDeBusca'
import { ResultadoDaBusca, type EstadoDaBusca } from './components/ResultadoDaBusca'

export default function App() {
  const [estado, setEstado] = useState<EstadoDaBusca>({ tipo: 'inicial' })
  const ultimaBusca = useRef<BuscaRequest | null>(null)
  const controlador = useRef<AbortController | null>(null)

  // Cancela uma busca em andamento se o componente for desmontado.
  useEffect(() => () => controlador.current?.abort(), [])

  const buscar = useCallback(async (request: BuscaRequest) => {
    controlador.current?.abort()
    const atual = new AbortController()
    controlador.current = atual
    ultimaBusca.current = request

    setEstado({ tipo: 'carregando' })

    try {
      const dados = await buscarProfessores(request, atual.signal)
      if (atual.signal.aborted) return
      setEstado({ tipo: 'sucesso', dados })
    } catch (erro) {
      if (atual.signal.aborted) return
      if (erro instanceof ErroDeValidacao) {
        setEstado({ tipo: 'erro-validacao', mensagem: erro.message, erros: erro.erros })
      } else if (erro instanceof ErroDeComunicacao) {
        setEstado({ tipo: 'erro-comunicacao', mensagem: erro.message })
      } else {
        setEstado({ tipo: 'erro-comunicacao', mensagem: 'Ocorreu um erro inesperado.' })
      }
    }
  }, [])

  const tentarNovamente = useCallback(() => {
    if (ultimaBusca.current) void buscar(ultimaBusca.current)
  }, [buscar])

  return (
    <main className="pagina">
      <header className="cabecalho">
        <h1>Encontre um Professor</h1>
        <p>
          Informe o idioma, os dias, a faixa de horário e a duração da aula. Mostramos quem pode
          atender no <strong>mesmo horário em todos os dias</strong> escolhidos.
        </p>
      </header>

      <FormularioDeBusca carregando={estado.tipo === 'carregando'} onBuscar={(r) => void buscar(r)} />

      <section className="area-resultado" aria-live="polite">
        <ResultadoDaBusca estado={estado} onTentarNovamente={tentarNovamente} />
      </section>
    </main>
  )
}
