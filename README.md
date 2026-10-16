# Desafio técnico: Encontre um Professor

Solução para o desafio de pessoa desenvolvedora júnior: uma API em **C# / .NET 8** que encontra
professores disponíveis para um novo aluno e uma tela em **React + TypeScript** para usá-la.

```
.
├── professores.json                 # dados (fonte única, usados pela API e pelos testes)
├── backend/
│   ├── EncontreProfessor.sln
│   ├── src/
│   │   ├── EncontreProfessor.Dominio/   # regra de negócio pura (sem HTTP, sem JSON)
│   │   └── EncontreProfessor.Api/       # ASP.NET Core: controller, carga do JSON, validação de entrada
│   └── tests/
│       └── EncontreProfessor.Tests/     # xUnit — regras, validação e cenários com os dados reais
├── frontend/                        # Vite + React + TypeScript
├── README.md
└── USO_DE_IA.md
```

## Como rodar

### Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (testado com 22)

### API

```bash
cd backend
dotnet restore
dotnet run --project src/EncontreProfessor.Api
```

A API sobe em **http://localhost:5080** (perfil `http` do `launchSettings.json`).
Em ambiente de desenvolvimento o Swagger fica em http://localhost:5080/swagger.

Teste rápido:

```bash
curl -s http://localhost:5080/api/professores/busca \
  -H "Content-Type: application/json" \
  -d '{"idioma":"Espanhol","dias":["segunda"],"inicioFaixa":"18:00","fimFaixa":"20:00","duracaoMinutos":60}'
```

### Testes

```bash
cd backend
dotnet test
```

Os testes não sobem a API: instanciam `BuscaDeProfessores` e `ValidadorDeBusca` diretamente.
O único arquivo externo que usam é o próprio `professores.json`, copiado para a pasta de saída.

### Tela

```bash
cd frontend
npm install
npm run dev
```

Abra **http://localhost:5173**. O Vite faz proxy de `/api` para `http://localhost:5080`, então
não é preciso configurar nada se a API estiver no endereço padrão. Para apontar para outro
endereço, crie um `.env` a partir de `.env.example` e preencha `VITE_API_URL`.

Build de produção: `npm run build` (saída em `frontend/dist`).

## Arquitetura

A separação pedida na avaliação ("regra de negócio separada da camada HTTP") é a espinha dorsal
do projeto:

| Camada | Projeto | O que faz |
| --- | --- | --- |
| Domínio | `EncontreProfessor.Dominio` | `BuscaDeProfessores` (regras 1–7), `ValidadorDeBusca` (seção 3), modelos (`Professor`, `Horario`, `DiaDaSemana`). Não referencia ASP.NET nem System.Text.Json. |
| HTTP | `EncontreProfessor.Api` | `ProfessoresController` traduz requisição → `ValidadorDeBusca` → `BuscaDeProfessores` → resposta. `RepositorioDeProfessoresJson` carrega o arquivo uma vez e mantém em memória. |
| Tela | `frontend` | `App` (máquina de estados), `FormularioDeBusca`, `ResultadoDaBusca`, cliente `api/professores.ts`. |

Pontos de implementação que valem destaque:

- **`Horario`** é um `record struct` com minutos desde a meia-noite. Toda a aritmética
  (sobreposição, "cabe no bloco", arredondar para `:00`/`:30`) vira comparação de inteiros,
  o que elimina uma classe inteira de bugs com strings.
- **Intervalos semiabertos `[início, fim)`.** É isso que torna "encostar é permitido" natural:
  `19:00–20:00` e `20:00–21:00` não se sobrepõem porque `inicio < fimOutro && inicioOutro < fim`
  falha na igualdade.
- **Horários candidatos vêm do aluno, não do professor.** Gero os inícios possíveis a partir da
  faixa pedida (arredondando o início para a próxima meia hora e parando quando a aula não cabe
  mais antes do fim da faixa) e, para cada um, verifico se está livre em **todos** os dias. Isso
  implementa as regras 3, 4 e 6 de uma vez e já entrega os horários em ordem crescente (regra 7).
- **Motivos de inelegibilidade** são constantes com o texto exato do enunciado, testados na ordem
  de prioridade pedida.

## Decisões tomadas (situações não cobertas pelo enunciado)

1. **Comparação de idioma sem distinguir caixa nem acento.** `"ingles"`, `"INGLÊS"` e `"Inglês"`
   encontram o mesmo professor (`CompareOptions.IgnoreCase | IgnoreNonSpace`). A tela só envia os
   três valores canônicos, mas a API não deveria falhar por um acento em chamadas diretas.
2. **Dias: aceito caixa e espaços diferentes (`" Segunda "`), mas não acento (`"terça"`).**
   O enunciado fixa a grafia `terca`/`sabado`; aceitar variações acentuadas abriria margem para
   "quase-iguais" silenciosos. A mensagem de 400 lista os valores válidos.
3. **Dias repetidos na requisição contam uma vez.** `["segunda","segunda"]` equivale a
   `["segunda"]`. Não é erro: a intenção do aluno é clara.
4. **Início da faixa fora de `:00`/`:30` (ex.: `18:15`)** não é erro de validação: o primeiro
   horário candidato passa a ser a próxima meia hora (`18:30`). O enunciado só exige `HH:mm`
   válido para a faixa; a restrição de meia hora é sobre o início **da aula**.
5. **Faixa menor que a duração (ex.: `18:00–18:30` com 60 min)** é uma requisição válida que não
   gera horário nenhum. Todos os professores elegíveis caem em
   `Sem horário em comum nos dias pedidos`. Preferi isso a inventar uma 5ª mensagem de 400 que o
   enunciado não prevê.
6. **Validação acumula todos os erros** em vez de parar no primeiro, e erros de *model binding*
   (ex.: `duracaoMinutos` enviado como texto, JSON malformado) são convertidos para o mesmo
   formato — `{ "mensagem": "...", "erros": ["..."] }` — para que a tela trate um único contrato.
7. **`aulasNaSemana` conta todas as aulas marcadas do professor**, em qualquer dia, não apenas
   nos dias pedidos. É o que "aulas marcadas na semana" diz literalmente, e bate com o exemplo
   do enunciado (Diego tem 1 aula na terça e aparece com `aulasNaSemana: 1` numa busca de segunda).
8. **Desempate alfabético ignora caixa e acento** (`InvariantCulture`), para que "Ábia" não vá
   parar depois de "Zeca".
9. **A tela não replica a validação da API.** O formulário envia o que o usuário preencheu (sem
   `required`, sem checagem de faixa) e exibe os erros 400 devolvidos. O enunciado pede
   explicitamente o estado "erros de validação devolvidos pela API"; duplicar as regras no front
   esconderia esse estado e criaria duas fontes de verdade. Para forçar um 400 na tela: desmarque
   todos os dias, ou coloque "Até" antes de "Das".
10. **Dados carregados uma vez na inicialização**, em um singleton. Um `professores.json`
    malformado (horário fora de `HH:mm`, dia inválido) derruba a aplicação no boot com mensagem
    indicando o professor — melhor do que responder errado em silêncio. O caminho do arquivo é
    configurável em `Professores:Caminho` (`appsettings.json`).

## Testes

`backend/tests/EncontreProfessor.Tests` — 3 classes, 50 testes no total (`dotnet test`: 50 aprovados, 0 falhas):

- **`BuscaDeProfessoresTests`** (16 testes): ao menos um por regra da seção 2, com os casos de
  fronteira: encostar vs. sobrepor, aula começando antes do bloco (`18:15`), aula terminando
  depois da faixa, interseção vazia entre dias, prioridade dos motivos, ordenação com empate.
- **`ValidadorDeBuscaTests`** (30 casos, a maioria via `[Theory]`): cada condição de 400 da seção 3.
- **`CenariosComDadosReaisTests`** (4 testes): roda a busca sobre o `professores.json` real,
  começando pelo exemplo exato do enunciado (Ana `["18:00"]`, Diego `["18:00","18:30","19:00"]`,
  6 não elegíveis por idioma).

## O que ficou faltando / próximos passos

- **Testes automatizados da tela** (Vitest + Testing Library) para os cinco estados. A lógica do
  front é pequena e foi verificada manualmente, mas não há suíte.
- **Teste de integração HTTP** com `WebApplicationFactory` cobrindo o 400 do model binding. A
  `partial class Program` já está exposta para isso.
- **Paginação / cache** não fazem sentido para 8 professores; se a base crescer, o primeiro
  passo seria indexar professores por idioma e dia.
- Não há Dockerfile nem CI; `dotnet test` e `npm run build` são os dois comandos de verificação.
