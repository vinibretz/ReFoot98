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

## [0.2.9] - 2026-11-06

### Novidades

- Configurações: o que vinha junto com os cartões amarelos virou opção separada na versão
  Atual: Nota e risco de lesão, Preço do ingresso (com a torcida pesando no árbitro) e Campinho
  e árbitro. As opções da página Regras ficam agrupadas pelo que afetam (Partida, Jogadores,
  Estádio e dinheiro, Tela da equipa), no Retrô, no Moderno e no celular. Quem já tinha as
  Configurações gravadas continua com tudo como estava.
- Moderno: na aba Escalação, clicar na formação no canto do campinho abre a lista de táticas
  (com Automático e Melhores) para escalar dali mesmo.

### Correções

- Moderno, tema claro: no campinho da Escalação, os nomes dos jogadores, a formação e
  "Titulares · Reservas" voltam a aparecer (estavam com letra escura sobre a etiqueta escura).
- Desligar os cartões amarelos não esconde mais a nota e o risco de lesão, o preço do ingresso,
  o campinho e as chances do árbitro.
- Preço do ingresso desligado (e na versão Retrô): o público e a bilheteria usam o preço de
  tabela, mesmo que o save tenha um preço escolhido antes; ao religar, ele volta.
- Preço do ingresso: um jogo salvo no meio da rodada e reaberto com a opção trocada refaz o
  público do jogo em casa dessa rodada com o preço que vale agora.
- Online: quem entra na sala joga com as opções da versão Atual do host (amarelos, nota e
  risco, preço do ingresso, campinho), não com as do próprio aparelho; a previsão do público
  não mostra mais um preço que o host não cobra.
- Configurações: a ajuda de "Nota e risco de lesão" avisa que o botão Comprar do Procurar
  também depende dela.
- Moderno 16:9: no resumo das Configurações, "Campinho" (e, em inglês e espanhol, "Sobem e
  descem") não fica mais espremido contra o valor.
- Clássico 4:3: nas Finanças, a previsão do público do próximo jogo em casa não sai mais cortada
  no fim (com um adversário de nome comprido, a ocupação sai da linha).
- Moderno: na ficha do jogador da tela do time, "Valor de mercado" e "Comportamento" aparecem
  inteiros (a letra encolhe um pouco em vez de cortar o rótulo).
- Inglês: o título da coluna dos gols na temporada do elenco ("Seas.") não fica mais por cima
  do de gols no Clássico 16:9.

### Interno

- Testes do preço do ingresso desligado, religado e na Retrô (`IngressosTestes`) e da torcida
  no árbitro sem os amarelos (`IngressosPublicoTestes`), do save reaberto com a opção trocada
  (`IngressosTestes.SaveReaberto*`) e das opções do host no online (`OnlineOpcoesDoHostTestes`).
- Testes sem o jogo original: os que leem saves reais rodam com os saves do próprio jogo e do
  SoakUI (`saves-locais.txt`; `GUARDAR_SAVE=caminho` no SoakUI guarda o save do fim do soak).
  `IngressosPublicoTestes` descarta os jogos do save em vez de jogá-los até o fim;
  `ClassificacaoNosSaves` aceita o desempate por vitórias; `SemAcrescimosIgualAoOriginal` tem a
  impressão de cada conjunto de saves. Os que precisam do jogo original (a pasta do jogo, o
  oráculo, os saves do launcher) aparecem como ignorados, com o motivo, quando os arquivos não
  existem (`Requisitos.cs`: `[FactComOriginal]`, `[FactComArquivo]`, `[TheoryDoOraculo]`): a
  suíte fica verde e uma falha é sempre um problema de verdade.
- `scripts/testar-tudo.ps1` (`docs/testes.md`, e a seção Testes do README): os testes do motor e da interface, três soaks
  (as opções da versão Atual ligadas, desligadas e o online) e as capturas de cada tela em cada
  visual, com uma página para passar o olho e o resumo em `dist\testes\`.
- `RetroFoot.App.Testes`: os testes da interface. As telas de verdade numa janela sem tela, em
  8 visuais, com os textos desenhados lidos de volta: nada sobreposto nem cortado (nos 4
  idiomas), cada opção mostrando só o que é dela, os interruptores e a versão Retrô travada nas
  Configurações, o `config.json`. A tela do time no celular abre nas Finanças ou no árbitro com
  `RETROFOOT_PAGINA` (também no FichasPrint).
- `ferramentas/FichasPrint`: capturas da tela do time (`RETROFOOT_PAGINA`), do Procurar e das
  Configurações (`RETROFOOT_SECAO`), em 4:3 ou 16:9 (`RETROFOOT_FORMATO`), com cada opção ligada
  ou desligada (`RETROFOOT_AMARELOS`, `_NOTA`, `_INGRESSO`, `_CAMPINHO`). SoakUI e SoakOnline:
  `SEM_AMARELOS`, `SEM_NOTA`, `SEM_INGRESSO`, `SEM_CAMPINHO`.

- `scripts/gerar-release.ps1`: libera o servidor de build antes de cada publish e tenta de novo
  uma vez se o publish ou o build do Android falhar (falta de memória do ILLink); o log do erro
  fica em `dist\`. Documentado em `docs/release.md` e no `CLAUDE.md`.
- `scripts/gerar-release.ps1 -SoAndroid`: gera só o APK, para trocar o Android de uma release
  já lançada sem mudar a versão.

## [0.2.8] - 2026-11-04

### Novidades

- Moderno: na aba Jogador, o botão Vender só aparece quando o jogador pode ser vendido (como
  já acontecia com o Renovar contrato), e no menu do botão direito sobre o jogador o Vender
  fica esmaecido. O motivo da recusa continua no menu Jogador > Vender.
- Atualização: o aviso de versão nova mostra também o que mudou (Novidades e Correções), de
  todas as versões entre a sua e a nova, no computador e no celular.

### Correções

- Moderno: na tela 4:3, a ficha do jogador (com a linha da Lei Bosman ou mais selos) não cobre
  mais os botões Vender e Renovar contrato; as linhas da tabela encolhem para caber.
- Moderno: na tela 4:3, a aba Finanças não fica mais embolada: o preço do ingresso e a previsão
  do próximo jogo em casa ganharam um cartão compacto no rodapé, com o público e a renda
  previstos inteiros, e o seletor de preço não encosta mais nas linhas do balanço.
- Android: na ficha do jogador, o botão Vender fica esmaecido quando o jogador não pode ser
  vendido, como o Renovar contrato (corrigido no APK da 0.2.8, trocado depois do lançamento).

### Interno

- `CLAUDE.md`: o script da release roda com o PowerShell 7 (`pwsh`).
- `docs/release.md`: a descrição da release precisa manter `### Novidades` e `### Correções`,
  que o aviso de atualização mostra ao jogador.

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
