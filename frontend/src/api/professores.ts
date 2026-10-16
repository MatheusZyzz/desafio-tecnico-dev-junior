import type { BuscaRequest, BuscaResponse, ErroDeValidacaoResponse } from './tipos'

const BASE_URL = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? ''

/** A API respondeu 400 com uma lista de erros de validação. */
export class ErroDeValidacao extends Error {
  readonly erros: string[]
  constructor(mensagem: string, erros: string[]) {
    super(mensagem)
    this.name = 'ErroDeValidacao'
    this.erros = erros
  }
}

/** Não foi possível falar com a API (rede fora, servidor desligado, 5xx, resposta inesperada). */
export class ErroDeComunicacao extends Error {
  constructor(mensagem: string) {
    super(mensagem)
    this.name = 'ErroDeComunicacao'
  }
}

export async function buscarProfessores(
  request: BuscaRequest,
  signal?: AbortSignal,
): Promise<BuscaResponse> {
  let resposta: Response
  try {
    resposta = await fetch(`${BASE_URL}/api/professores/busca`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    })
  } catch (erro) {
    if (erro instanceof DOMException && erro.name === 'AbortError') throw erro
    throw new ErroDeComunicacao('Não foi possível conectar à API. Verifique se ela está em execução.')
  }

  if (resposta.status === 400) {
    const corpo = await lerJson<Partial<ErroDeValidacaoResponse>>(resposta)
    const erros = Array.isArray(corpo?.erros) && corpo.erros.length > 0
      ? corpo.erros
      : ['A API rejeitou a requisição, mas não detalhou o motivo.']
    throw new ErroDeValidacao(corpo?.mensagem ?? 'Requisição inválida.', erros)
  }

  if (!resposta.ok) {
    throw new ErroDeComunicacao(`A API respondeu com erro ${resposta.status}.`)
  }

  const dados = await lerJson<BuscaResponse>(resposta)
  if (!dados || !Array.isArray(dados.professores)) {
    throw new ErroDeComunicacao('A API devolveu uma resposta em formato inesperado.')
  }
  return { professores: dados.professores, naoElegiveis: dados.naoElegiveis ?? [] }
}

async function lerJson<T>(resposta: Response): Promise<T | null> {
  try {
    return (await resposta.json()) as T
  } catch {
    return null
  }
}
