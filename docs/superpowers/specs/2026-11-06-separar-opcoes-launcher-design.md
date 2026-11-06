# Separar as opções que hoje vêm junto com "Cartões amarelos"

## Intenção

Na versão Atual (Modificada), o interruptor "Cartões amarelos" (`Configuracoes.CartoesAmarelos` →
`Regras.CartoesAmarelos`) liga e desliga, além dos amarelos, tudo o que os executáveis do launcher
acrescentavam. No código a flag é lida como `_launcher`/`launcher` em várias telas. O jogador
espera mexer só nos amarelos e acaba mudando outras coisas. Cada recurso precisa ter a
própria opção.

## Opções (decididas com o vinibretz)

| Opção (rótulo pt-BR) | O que controla |
|---|---|
| **Cartões amarelos** | Só os amarelos: acúmulo e suspensão com 3 (`FimDeJogo.DescontarComAmarelos`), zerar na temporada (`JogoEmCurso` linha ~238), amarelos na partida (`Jornada.Amarelos`), o "A" / "N amarelos acumulados" e a linha "Cartões amarelos" no histórico das fichas, e o parâmetro de amarelos da `TelaSubstituicao`. |
| **Nota e risco de lesão** (nova) | Nota e RL nas fichas e listas de jogador (TelaEquipa, TelaEquipaModerna, TelaEquipaCelular, TelaPlantel, TelaPlantelCelular, TelaLeilao, TelaLeilaoCelular, TelaObservados(Celular), MercadoCelular, MercadoRetroCelular, MercadoComum) e, no Procurar (TelaProcura, TelaProcuraCelular), os filtros extras, a ordem por Nota e RL e a largura extra (`_mais`). |
| **Preço do ingresso** (nova) | O seletor de preço em Finanças (setas, roda, `RIngressos`, `_mIngressos`, o cartão de ingresso do Moderno, o bloco do celular, a previsão `IngressosPrevisao`) **e** a torcida no árbitro (`Jogos.BonusDaTorcida`). |
| **Campinho e árbitro** (nova) | O campinho na tela do time (`TelaEquipa.Campinho`) e o árbitro completo, com a chance de pênalti de cada time (`ArbitroCompleto`; continua `|| Registo.AddIn(0x23)`). A frase "Pênalti: X%   Cartões: -N%" (`TelaEquipa.TimeArbitro` ~1697, celular ~352) aparece inteira mesmo com os amarelos desligados (**decisão do Claude**: o árbitro também deixa de dar vermelhos). |

Continuam sempre ligados na Atual e fora destas opções: a tática 5-0-5, o limite de 30
jogadores e o desempate por vitórias (`Regras.Launcher(n)`).

### Preço do ingresso desligado (no motor)

Desligada, a opção vale o **preço de tabela**, como no Retrô, mesmo que o save tenha um preço
escolhido antes (de quando a opção estava ligada):
- `Jogos.cs` (linhas ~155, ~230 e ~233): não aplicar `Ingressos.Publico`; usar a procura de tabela.
- `FimDeJogo.cs` (linhas ~107 e ~141): a bilheteria usa o preço de tabela, não `Ingressos.Preco(..., casa)`
  com a razão escolhida.
- `BonusDaTorcida`: devolve 2 (já é o que acontece sem escolha).
- A razão guardada no save **não é apagada**: religar a opção traz de volta o preço escolhido.
- `Online/Comandos.cs`: o comando de razão continua aceito (só grava); o efeito depende da regra.
- **Decisão do Claude:** a versão Retrô (`Regras.Original`, opção desligada) também passa a ignorar
  um preço gravado no save (vindo do launcher ou de uma carreira na Atual): público e bilheteria
  de tabela. É o comportamento do jogo de 1998, que grava esses bytes zerados. Precisa de teste
  também para o Retrô. Os oráculos não mudam (saves do original com os bytes zerados).

## Configurações: opções agrupadas pelo que afetam (aprovado no esboço)

O cartão/lista única "Adições" da página **Regras** vira grupos:

- **Partida:** Cartões amarelos, Acréscimos, 5 substituições, 9 reservas.
- **Jogadores:** Nota e risco de lesão.
- **Estádio e dinheiro:** Preço do ingresso, Patrocínios, Premiação ampliada.
- **Tela do time:** Campinho e árbitro.

Por modelo:
- **Clássico (janela Windows 95, `TelaConfiguracoes.MontarClassico`):** tamanho fixo (588×352) no 4:3
  e no 16:9. Dois grupos (`Grupo`) lado a lado no topo da página Regras: **Partida** (4 marcas)
  e **Clube** (Patrocínios, Preço do ingresso, Premiação ampliada, Nota e risco de lesão,
  Campinho e árbitro). Abaixo deles: sobem e descem, a ajuda, Jogadores estrangeiros e Desempate,
  apertados verticalmente para caber (o desenho aprovado: grupos em y≈40 até ≈157, combo ≈164,
  ajuda ≈188, estrangeiros ≈216–274, desempate ≈278–320). O aviso vermelho da Retrô (y=8) continua.
- **Moderno (`MontarModerno`):** quatro `Cartao` na ordem acima, cada um com título e subtítulo
  ("O que acontece em campo." / "O que você vê de cada jogador." / "Bilheteria, patrocínio e
  prêmios." / "Informações extras na preparação do jogo."), interruptores com `LinhaInterruptor`.
  O subtítulo da navegação da secção Regras ("Cartões, patrocínios, estrangeiros") pode ser ajustado.
- **Celular Moderno (`TelaConfiguracoesCelular.RegrasCelular`):** quatro `CCfg.Bloco` na mesma ordem.
- **Celular Retrô (`TelaConfiguracoesCelularRetro`, case 1):** as mesmas divisões com `CUi.GrupoRetro`
  e `CR95.Marca`.

Em todos: na versão Retrô as marcas aparecem desligadas e travadas (como hoje), e o texto
"Cartões amarelos, patrocínios e acréscimos só existem na versão Atual…" passa a citar as adições
de forma genérica (ex.: "As adições só existem na versão Atual…").

Textos de ajuda (Moderno e celular):
- Nota e risco de lesão: "A nota e o risco de lesão nas fichas e listas de jogadores, e os filtros e a ordem por eles no Procurar."
- Preço do ingresso: "Você escolhe o preço nos jogos em casa (em Finanças); o público que ele atrai pesa no árbitro. Desligado: preço de tabela, como no Retrô."
- Campinho e árbitro: "O campinho com a escalação e, no árbitro, a chance de pênalti para cada time."
- Cartões amarelos: mantém o texto atual.

## Dados e regras

- `Configuracoes`: três `bool` novos (`NotaERisco`, `PrecoIngresso`, `CampinhoArbitro`; nomes
  livres), gravados no JSON (`notaERisco`, `precoIngresso`, `campinhoArbitro`).
- **Migração:** chave ausente no arquivo → recebe o valor de `cartoesAmarelos` lido do mesmo
  arquivo. Quem já tinha configuração não vê mudança até mexer.
- `Regras` (record do motor): três `init` novos com o mesmo significado. `Regras.Original` = todos
  falsos. `Regras.Launcher(n)` = todos verdadeiros (mantém os testes e ferramentas que usam
  `Launcher(8)` como "tudo do launcher"). `Configuracoes.RegrasPara` passa os valores da
  configuração, como já faz com `CartoesAmarelos`.
- As telas trocam `_launcher = rg.CartoesAmarelos` pela flag específica de cada recurso. Onde uma
  tela usa `_launcher` para mais de um recurso (sobretudo TelaEquipa, TelaEquipaModerna,
  TelaProcura, TelaLeilao), separar em flags por recurso.
- Ferramentas de demonstração que montam `Regras.Launcher(8)` via `RETROFOOT_LAUNCHER=1` continuam
  com tudo ligado.
- SoakUI / SoakOnline (`Program.cs`, linhas ~62/~64): ligar as opções novas junto com `mod`, e
  permitir combinações misturadas para as capturas (variável de ambiente ou parâmetro, à escolha
  de quem implementar).

## Layout nas telas do jogo

Hoje cada tela só conhece dois estados (tudo ligado / tudo desligado). Com as opções
separadas aparecem combinações novas. Toda posição que dependia de `_launcher` precisa ser
revista para a flag certa, e as combinações misturadas não podem sobrepor nem cortar texto.
Casos conhecidos:
- **TelaEquipa (Clássico):** na página Jogador (ficha), `ys/yl` (linha ~1439: amarelos acumulados
  vs suspensão) passa a depender de amarelos (e da Nota e Risco acima, se deslocar); em Finanças,
  `yt/yj/yc` (linha ~1479: os totais sobem para caber a previsão do ingresso) passa a depender
  do ingresso.
- **TelaEquipa, ficha:** Nota e Risco abaixo do Comportamento (linhas ~1386–1420), colunas da lista
  16:9 (~1117–1155).
- **TelaEquipaModerna:** `cartaoIngresso` (linha ~902) e a altura do árbitro (~1106).
- **TelaProcura:** `_mais`, posições dos botões (~210, ~223), `Ordens`.
- **TelaLeilao / TelaPlantel:** `passo` e as linhas da ficha.

### Verificação

Capturas com o SoakUI (no máximo ~6 em paralelo; sem o jogo original,
`RETROFOOT_JOGO=RetroFoot.App/Assets/Dados`) cobrindo:
- visuais: Clássico, Moderno verde/escuro/claro, celular Moderno e celular Retrô;
- formatos: 4:3 e 16:9 (computador);
- combinações: todas ligadas, todas desligadas, e misturas (ex.: só ingresso; só nota e risco;
  amarelos sem ingresso; ingresso sem amarelos);
- telas: Configurações (Regras), Time (ficha, Finanças, árbitro, campinho), Elenco, Leilão,
  Observados, Procurar.

Testes do motor: preço do ingresso desligado → público e bilheteria de tabela com uma razão
gravada no save; ligado → como hoje (`IngressosPublicoTestes`). Amarelos desligados com ingresso
ligado → torcida no árbitro continua funcionando. Migração do JSON de configuração.

## Resumos

Linhas novas em: `TelaConfiguracoes.Resumo()` (Moderno, coluna da direita no largo),
`RegrasDoJogo` ("Jogo em curso"), `TelaInicioModerna` (linha ~176) e o resumo do celular
(`TelaConfiguracoesCelular`, linha ~91).

## Traduções

Os textos novos entram nos arquivos de idioma (`RetroFoot.App/Assets/Idiomas/{pt-BR,pt-PT,en,es}`),
seguindo `docs/glossario-idiomas.md`.

## Fora do escopo

- Online: as regras continuam saindo das configurações de quem abre o jogo, como os amarelos
  hoje. Não mexer.
- Tática 5-0-5, limite de 30 e desempate por vitórias continuam presos à versão Atual.

## Registro

Entrada no `CHANGELOG.md`, [Não lançado] → Novidades: as opções separadas nas Configurações.
