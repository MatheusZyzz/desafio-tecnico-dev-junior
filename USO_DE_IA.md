# Uso de IA

**Ferramenta:** Claude (Anthropic), via app Claude. Única ferramenta de IA usada.

**Como usei:** como par de programação durante todo o desafio — para estruturar a solução em
camadas (domínio, API, tela, testes), gerar a primeira versão dos arquivos a partir das decisões
de modelagem que fechei antes (horário em minutos, intervalos semiabertos, candidatos gerados a
partir da faixa do aluno, motivos como constantes na ordem de prioridade) e escrever a documentação.
As decisões sobre casos não cobertos pelo enunciado são minhas e estão no `README.md`.
Compilação, testes (`dotnet test`: 50/50), API e tela foram executados e verificados por mim no
Windows.

**Onde a IA errou:** no teste `Regra4_AulaNaoPodeSeEspalharPorDoisBlocosSeparados` (dois blocos
adjacentes `18:00–19:00` e `19:00–21:00`, faixa `18:00–21:00`, aula de 60 min), a primeira versão
trazia como esperado `["18:00", "19:00", "20:00"]`. Faltava `19:30`, que cabe inteiro no segundo
bloco. Percebido na revisão, contando os slots de 30 em 30 minutos antes de rodar os testes; se
tivesse apenas visto o teste falhar, o risco era "corrigir" a implementação, que estava certa.

**Limitação registrada:** o ambiente da IA não tinha o SDK do .NET, então as regras foram
validadas primeiro com uma implementação de referência em Node sobre o `professores.json` real, e
o C# foi compilado e testado na minha máquina.
