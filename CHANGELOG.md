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

## [0.2.10] - 2026-11-07

### Novidades

- Online: o nome do técnico passou a ser obrigatório. Sem ele, criar, entrar ou continuar uma
  sala mostra um aviso e marca o campo do nome.
- Online: a tela da sala mostra as regras do jogo, com uma explicação de como elas funcionam.
  O host pode mudar ali, antes de começar ou continuar, as que não atrapalham uma temporada em
  andamento: cartões amarelos, nota e risco, preço do ingresso, campinho, patrocínios, 5
  substituições, 9 reservas, premiação ampliada, substituições no intervalo, pênaltis da CPU e
  estrangeiros. A versão (Retrô ou Atual), os acréscimos, as equipas por divisão e quantas sobem e descem aparecem também, mas não mudam: numa
  sala nova, a tela avisa que vêm das Configurações (para mudar, saia, ajuste no menu e crie a sala
  de novo); num jogo continuado, que ficaram fixas na criação. Os convidados veem o resumo.
- Online: a tela de entrada e a tela da sala usam melhor o espaço: com a janela larga, os
  cartões ficam em duas colunas (a chave e os técnicos de um lado, as regras do outro); no
  celular e em janelas estreitas, continuam um embaixo do outro.

### Correções

- Online: com o goleiro expulso e nenhuma troca possível (substituições esgotadas ou banco
  vazio), o jogo segue direto, sem parar a partida de todos por até 30 segundos.
- Online: o convidado agora vê o patrocinador na aba Patrocínio (a marca, o logo, o valor e as
  temporadas) e o logo da marca na proposta. Antes a aba dizia "Sem patrocinador" mesmo com
  contrato assinado.
- Online: a lesão de um jogador do convidado sem ninguém no banco que pudesse entrar (por
  exemplo, só goleiro no banco e saiu um jogador de linha) parava a partida de todos: o
  convidado fechava a janela sem resposta e o host ficava esperando. Agora a janela nem abre
  (como já acontecia com o banco vazio), e o convidado sempre responde à janela da lesão.
- Online: a pausa de substituições do convidado sempre devolve o jogo ao host, mesmo se a
  internet falhar ao mandar uma troca (antes o jogo ficava parado até o fim do prazo).
- Online: as janelas que só o host via agora aparecem também para os convidados: as
  demissões de técnicos (quem foi demitido fica sabendo na hora), o fim de temporada (campeão,
  copa e artilheiro), a premiação das divisões e as subidas e descidas. Elas não seguram o jogo: se a rodada seguinte chegar com uma delas aberta, ela continua por cima.
- Online: o convidado demitido vê "Você está sem clube…" em vez de "Aguardando o host dar um
  time para você…".
- Online: a janela das demissões de técnicos no host tem o prazo das outras janelas; antes,
  se o host não fechasse, o jogo ficava parado para todos.
- Online: o convidado joga com todas as regras do host, não só com as quatro opções da versão
  Atual: a versão (Retrô ou Atual), 5 substituições, 9 reservas, o desempate por vitórias, os
  acréscimos, os patrocínios e os estrangeiros. Antes, com Configurações diferentes, a tela do
  convidado podia deixar escalar ou substituir mais do que o host aceitava.
- Online: as Opções do host não decidem mais escondido pelos convidados. As demissões de
  técnicos aparecem em cada aparelho conforme as Opções dele (antes, com o host em "Nunca", o
  convidado nem ficava sabendo da própria demissão). As substituições no intervalo e os pênaltis
  dos jogos sem técnico humano viraram regras da sala, que o host escolhe e valem para todos
  (antes o host podia ficar sem intervalo e o convidado com); nas Opções, durante o jogo online,
  essas duas ficam esmaecidas.
- Online: o sorteio da Copa não aparece mais no jogo online. Com o host em "Ver sorteio da copa"
  (o padrão é "quando houver humanos"), a animação equipa por equipa levava minutos com o jogo
  parado para todos. Nas Opções, durante o jogo online, essa opção fica esmaecida.
- Online: as regras de um jogo online ficam fixas desde a criação. Mudar as Configurações entre
  uma sessão e outra não muda mais as regras do jogo ao reabrir a sala, e o técnico que assume o
  host continua com as regras do jogo, não com as do aparelho dele.

### Interno

- `CLAUDE.md`, `docs/multiplayer-online.md` e comentários no código: toda opção nova que muda o
  jogo entra também nas regras do jogo online (o que atualizar junto).
- `RetroFoot.App.Testes`: a entrada do online e a sala (do host e do convidado, numa sala do
  banco da memória) entram nos testes de layout; o `Despejo` abre essas telas (`online`, `sala`,
  `salaconvidado`) e grava a imagem de cada visual com `DESPEJO_PNG=1`.
- Teste de todas as regras do host pela sala (`OnlineOpcoesDoHostTestes.TodasAsRegrasPelaSala`).
- SoakOnline: registra as janelas que o host publica para os convidados.

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
