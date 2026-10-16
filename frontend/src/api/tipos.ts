// Contrato de POST /api/professores/busca, espelhando a API.

export const IDIOMAS = ['Inglês', 'Espanhol', 'Francês'] as const
export type Idioma = (typeof IDIOMAS)[number]

export const DIAS = [
  { valor: 'segunda', rotulo: 'Segunda' },
  { valor: 'terca', rotulo: 'Terça' },
  { valor: 'quarta', rotulo: 'Quarta' },
  { valor: 'quinta', rotulo: 'Quinta' },
  { valor: 'sexta', rotulo: 'Sexta' },
  { valor: 'sabado', rotulo: 'Sábado' },
  { valor: 'domingo', rotulo: 'Domingo' },
] as const
export type Dia = (typeof DIAS)[number]['valor']

export const DURACOES = [30, 60, 90] as const
export type Duracao = (typeof DURACOES)[number]

export interface BuscaRequest {
  idioma: string
  dias: string[]
  inicioFaixa: string
  fimFaixa: string
  duracaoMinutos: number
}

export interface ProfessorEncontrado {
  id: number
  nome: string
  aulasNaSemana: number
  horarios: string[]
}

export interface ProfessorNaoElegivel {
  id: number
  nome: string
  motivo: string
}

export interface BuscaResponse {
  professores: ProfessorEncontrado[]
  naoElegiveis: ProfessorNaoElegivel[]
}

export interface ErroDeValidacaoResponse {
  mensagem: string
  erros: string[]
}
