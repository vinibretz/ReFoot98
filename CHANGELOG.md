# Novidades do ReFoot98

Tudo o que muda no jogo fica registrado aqui. As mudanças ainda não lançadas ficam em
**Não lançado**; ao lançar uma versão, essa seção vira a da versão (passo a passo em
`docs/release.md`) e é a base do commit, da tag e da descrição da release no GitHub.

Seções de cada versão: **Novidades** (o que o jogador ganha), **Correções** (o que estava
errado e foi consertado) e **Interno** (testes, documentação, ferramentas: não vai para a
descrição da release). Escreva para o jogador, em uma linha por item.

As versões até a 0.2.6 estão descritas nas [releases do GitHub](https://github.com/vinibretz/ReFoot98/releases).

## [Não lançado]

### Novidades

### Correções

### Interno

## [0.2.7] - 2026-11-03

### Novidades

- Online: quem tenta entrar numa sala de outra versão do jogo é avisado para atualizar e não
  entra (jogar com versões diferentes podia dar problemas). O host que atualiza o jogo e reabre
  a sala passa a versão nova para ela.

### Correções

- Lista de Jogadores (Procurar): depois de ser demitido e contratado por outro time, a busca não
  mostrava mais os jogadores do time antigo (a lista anterior, que escondia o "seu" time de
  então, voltava com o Procurar desligado). Agora a busca é refeita para o time novo.
- Online: a janela "Premiação das divisões" (premiação ampliada, no fim da temporada) agora
  fecha sozinha em 30 s no jogo online; antes, se o host não clicasse OK, o jogo ficava parado
  para todos.
- Online: ao continuar uma sala que passou para outro host, o jogo gravado não some mais da
  lista "Suas salas" se a entrada falhar.
- Online: no intervalo, as substituições e o "Continuar" dos convidados não esperam mais o host
  fechar a janela dele. Antes, quem terminava primeiro ficava sem resposta, o jogo parecia
  travado ("os técnicos fazem as substituições") e uma troca feita perto do fim dos 30 s podia
  se perder. Quem já terminou vê "aguardando os outros técnicos…".

### Interno

- Teste do intervalo com a janela do host aberta (`IntervaloAtendeComAJanelaDoHostAberta`).
- Testes da versão da sala (`VersaoSalaTestes`).
- Os testes online rodam sem o save do jogo original: basta copiar um save do jogo para
  `RetroFoot.Motor.Testes/Oraculo/j3-entrada.e98` (nota no `LEIA.md` dessa pasta).
- `CLAUDE.md` do projeto com as regras do modo online, deste registro de novidades e dos soaks.
- `docs/multiplayer-online.md`: o prazo das janelas do motor no host, a regra da versão e o
  intervalo de todos.
- `docs/release.md`: o script da release roda com o PowerShell 7 (`pwsh`).
