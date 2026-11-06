# Separar as opções do launcher: plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Separar o interruptor "Cartões amarelos" em quatro opções: Cartões amarelos, Nota e risco de lesão, Preço do ingresso, Campinho e árbitro. Cada uma passa a valer no motor, nas telas e nas Configurações de todos os modelos.

**Architecture:** O record `Regras` (motor) ganha três `init` booleanos, preenchidos por `Configuracoes.RegrasPara`. O motor passa a respeitar `PrecoIngresso` (público, bilheteria e torcida no árbitro). Toda leitura de `Regras.CartoesAmarelos` nas telas que, na verdade, significava "launcher" é trocada pela flag do recurso. As Configurações agrupam as opções por área.

**Tech Stack:** C# / .NET, Avalonia (headless nas ferramentas), xUnit (`RetroFoot.Motor.Testes`).

**Spec:** `docs/superpowers/specs/2026-11-06-separar-opcoes-launcher-design.md`

## Global Constraints

- **Não commitar.** O vinibretz testa localmente antes (CLAUDE.md global). Os passos de "commit" deste plano são substituídos por "deixar as mudanças sem commit".
- Trabalhar na pasta principal do projeto, sem worktree.
- Nomes: `Regras.NotaERisco`, `Regras.PrecoIngresso`, `Regras.CampinhoArbitro`. Em `Configuracoes`, as propriedades têm os mesmos nomes e as chaves JSON são `notaERisco`, `precoIngresso` e `campinhoArbitro`.
- `Regras.Original`: as três opções desligadas. `Regras.Launcher(n)`: as três ligadas.
- Migração: chave ausente no JSON → recebe o valor de `cartoesAmarelos` do mesmo arquivo.
- Na versão Retrô as quatro opções ficam desligadas e travadas nas Configurações. O `_cfg` guarda a escolha para a Atual, como as outras adições.
- Grupos e rótulos (pt-BR) exatamente como na spec: **Partida**, **Jogadores**, **Estádio e dinheiro**, **Tela do time** (no Clássico Win95: **Partida** e **Clube**).
- Textos fonte em pt-PT (o padrão do `T()`), com tradução em `Assets/Idiomas/{pt-BR,pt-PT,en,es}` segundo `docs/glossario-idiomas.md`.
- Soaks e capturas: no máximo ~6 processos em paralelo. Sem o jogo original, `RETROFOOT_JOGO=RetroFoot.App/Assets/Dados`.
- Toda mudança entra no `CHANGELOG.md`, em [Não lançado].

## Review Focus

1. **Save com preço escolhido e opção desligada** (inclusive na Retrô): público e bilheteria de tabela; ao religar, volta o preço gravado. → testes na Task 1.
2. **Ingresso ligado com amarelos desligados**: a torcida continua pesando no árbitro. → teste na Task 1.
3. **Config antiga sem as chaves novas** (`cartoesAmarelos: true` ou `false`): as três opções herdam o valor. Uma config já com as chaves novas é respeitada. → verificação na Task 2.
4. **Combinações misturadas nas telas** (ex.: só Nota e risco; só ingresso), no 4:3 e no 16:9: nada sobreposto nem cortado. → matriz de capturas na Task 6.
5. **Procurar com ordem Nota/RL escolhida e a opção depois desligada**: a ordem volta ao nome sem estourar o índice (o mesmo caso que hoje existe ao trocar de save). → verificação na Task 4.

---

### Task 1: Regras e motor (preço do ingresso)

**Files:**
- Modify: `RetroFoot.Motor/Regras/Regras.cs` (props novas; `Launcher(n)`)
- Modify: `RetroFoot.Motor/Rodada/Jogos.cs:155,168,230,233`
- Modify: `RetroFoot.Motor/Rodada/FimDeJogo.cs:107,141`
- Test: `RetroFoot.Motor.Testes/IngressosTestes.cs`, `RetroFoot.Motor.Testes/IngressosPublicoTestes.cs`

**Interfaces:**
- Produces: `public bool NotaERisco { get; init; }`, `public bool PrecoIngresso { get; init; }`, `public bool CampinhoArbitro { get; init; }` em `Regras`. `Regras.Launcher(n)` devolve `new(n, 30, true, true) { NotaERisco = true, PrecoIngresso = true, CampinhoArbitro = true }`.
- Produces (privado em `Mundo`): `Equipa? MandanteComPreco(Equipa? casa) => Regras.PrecoIngresso ? casa : null;`. Passar `null` a `Ingressos.Publico` ou `Ingressos.Preco` já devolve o valor de tabela.

- [ ] **Step 1: Ajustar os testes existentes que dependem do preço com a regra Original.** Em `IngressosTestes.Mundo2`, depois de criar o `Mundo`, faça `m.Regras = m.Regras with { PrecoIngresso = true }` (`Mundo.Regras` é um campo público). Os testes de `Ingressos.*` estático não mudam.
- [ ] **Step 2: Escrever os testes novos (falhando)** em `IngressosTestes`:
  - `PrecoDesligadoUsaTabela`: com `Mundo2(12)` e `PrecoIngresso = false`, `m.Publico(j)` dá o mesmo `D40` de `Mundo2(0)`, e `FimDeJogo` com `D40 = 12345` e preço 7 lança `12345 * 7` na bilheteria.
  - `PrecoReligadoVoltaOEscolhido`: no mesmo mundo, desliga, calcula, religa (`PrecoIngresso = true`) e o público e a bilheteria voltam aos de `escolha = 12`. A razão no cabeçalho continua igual (`casa.Cabeca[4] == 12`).
  - `RetroIgnoraPrecoGravado`: `new Mundo(s, semente)` sem mexer nas regras (Original) e `escolha = 12` → público e bilheteria de tabela.
  - Em `IngressosPublicoTestes`, `TorcidaSemAmarelos`: `Cenario` com `TRegras.Launcher(8) with { CartoesAmarelos = false }`; o bônus do árbitro muda com o preço, como em `TorcidaPesaNoArbitro`.
- [ ] **Step 3: Rodar e ver falhar.** `dotnet test RetroFoot.Motor.Testes --filter "FullyQualifiedName~Ingressos"`. Esperado: os novos falham por compilação (props inexistentes) ou por asserção.
- [ ] **Step 4: Implementar.** Props em `Regras` (com comentário curto no estilo do arquivo), `Launcher(n)` como em Interfaces e `MandanteComPreco` em `Mundo`. Use-o nas chamadas de `Ingressos.Publico`/`Ingressos.Preco` de `Jogos.cs` (155, 230, 233) e `FimDeJogo.cs` (107, 141). Em `BonusDaTorcida` (Jogos.cs:168), troque `Regras.CartoesAmarelos` por `Regras.PrecoIngresso`.
- [ ] **Step 5: Rodar a suíte inteira do motor.** `dotnet test RetroFoot.Motor.Testes`. Esperado: tudo passa, inclusive Oráculo e Online (usam `Launcher(8)` ou saves com bytes zerados).

### Task 2: Configurações (dados, migração, regras) e soaks

**Files:**
- Modify: `RetroFoot.App/Configuracoes.cs:35-38,81-84,198-210,223-235`
- Modify: `ferramentas/SoakUI/Program.cs:62`, `ferramentas/SoakOnline/Program.cs:64`

**Interfaces:**
- Consumes: as props da Task 1.
- Produces: `Configuracoes.NotaERisco`, `.PrecoIngresso`, `.CampinhoArbitro` (`bool`). `RegrasPara` passa as três no `with`, como faz com `CartoesAmarelos`.

- [ ] **Step 1: Implementar as props, a gravação e a leitura.** Em `Ler`, depois de ler `cartoesAmarelos`: `c.NotaERisco = Logico(o, "notaERisco") ?? c.CartoesAmarelos;` (o mesmo para as outras duas). Atualize o comentário do topo da classe que lista as adições.
- [ ] **Step 2: Soaks.** No `cfg` do SoakUI e do SoakOnline, as três props novas seguem `mod`. Cada uma pode ser desligada por variável de ambiente: `SEM_AMARELOS=1`, `SEM_NOTA=1`, `SEM_INGRESSO=1`, `SEM_CAMPINHO=1`.
- [ ] **Step 3: Compilar.** `dotnet build RetroFoot.Desktop` e `dotnet build ferramentas/SoakUI`. Esperado: 0 erros.
- [ ] **Step 4: Verificar a migração (Review Focus 3).** Com o executável de uma ferramenta que chama `Configuracoes.Ler(caminho)` (ex.: um `dotnet run` de um programa temporário na pasta de rascunho, fora do repositório), leia três JSONs: `{"versao":"modificada","cartoesAmarelos":true}` → as três `true`; `{"versao":"modificada","cartoesAmarelos":false}` → as três `false`; `{"versao":"modificada","cartoesAmarelos":false,"precoIngresso":true}` → só `PrecoIngresso` `true`. Apague o programa temporário depois.

### Task 3: Telas de Configurações (4 modelos), resumos e traduções

**Files:**
- Modify: `RetroFoot.App/TelaConfiguracoes.cs` (`MontarModerno` 164-236, 361-412; `Resumo` 425-442; `RegrasDoJogo` 134-151; `MontarClassico` 609-699)
- Modify: `RetroFoot.App/TelaConfiguracoesCelular.cs` (`RegrasCelular` 135-174; resumo ~91)
- Modify: `RetroFoot.App/TelaConfiguracoesCelularRetro.cs` (case 1, 75-106)
- Modify: `RetroFoot.App/TelaInicioModerna.cs:~176`
- Modify: `RetroFoot.App/Assets/Idiomas/{pt-BR,pt-PT,en,es}/configuracoes.txt`

**Interfaces:**
- Consumes: as props de `Configuracoes` (Task 2) e de `Regras` (Task 1, para `RegrasDoJogo` e `TelaInicioModerna`).

- [ ] **Step 1: Moderno.** Substituir o cartão "Adições" por quatro `Cartao`, com títulos e subtítulos da spec e `LinhaInterruptor` para cada opção (textos de ajuda da spec). Ligar os eventos e o `_atualizar` (habilitar/opacidade/marcado na Retrô) nos quatro grupos, como hoje em `adicoes`. Trocar o texto de `travado` por um genérico ("As adições só existem na versão Atual. A escolha fica guardada para quando você usar a Atual."). Ajustar `subt[1]` da navegação, por exemplo "Partida, jogadores, estádio".
- [ ] **Step 2: Clássico Win95.** Dois `Grupo` lado a lado em `pRegras`: Partida em (12, 36, 226, 118) e Clube em (246, 36, 226, 118), com as marcas a cada ~19 px. Depois, nas posições da spec: combo de sobem e descem ≈164, ajuda ≈188, estrangeiros ≈216–274, desempate ≈278–320. Atualizar o `trav` vermelho para o texto genérico. Aceita-se ajustar alguns pixels para não sobrepor; o critério é a captura da Task 6.
- [ ] **Step 3: Celular Moderno e celular Retrô.** Quatro `CCfg.Bloco` (Moderno) e quatro `CUi.GrupoRetro` com `CR95.Marca` (Retrô), na ordem da spec.
- [ ] **Step 4: Resumos.** `Resumo()` ganha as linhas "Nota e risco", "Ingresso" (Escolhido/Tabela) e "Campinho e árbitro". `RegrasDoJogo` e `TelaInicioModerna` ganham uma parte para cada opção ligada. O resumo do celular segue o do Moderno.
- [ ] **Step 5: Traduções.** Cada texto novo passado a `T()` entra nos quatro arquivos de idioma, com a mesma chave.
- [ ] **Step 6: Compilar e abrir.** `dotnet build RetroFoot.Desktop`. Esperado: 0 erros. A conferência visual fica na Task 6.

### Task 4: Nota e risco de lesão (fichas, listas e Procurar)

**Files:**
- Modify: `RetroFoot.App/TelaEquipa.cs` (ficha 1386-1442, lista 1048-1155), `RetroFoot.App/TelaEquipaModerna.cs`, `RetroFoot.App/TelaEquipaCelular.cs:426,675,873,899,942,955,963`
- Modify: `RetroFoot.App/TelaPlantel.cs:51,64,283,345`, `RetroFoot.App/TelaPlantelCelular.cs:189,354`
- Modify: `RetroFoot.App/TelaLeilao.cs:72,100,389-590`, `RetroFoot.App/TelaLeilaoCelular.cs:34-92`, `RetroFoot.App/TelaObservadosCelular.cs:156`
- Modify: `RetroFoot.App/MercadoCelular.cs:157-189`, `RetroFoot.App/MercadoRetroCelular.cs:246-263`
- Modify: `RetroFoot.App/TelaProcura.cs` (92-488), `RetroFoot.App/TelaProcuraCelular.cs:115,291`

**Interfaces:**
- Consumes: `Regras.NotaERisco`, `Regras.CartoesAmarelos`.
- Produces: as fichas compartilhadas passam a receber duas flags no lugar de `bool launcher`: `MercadoCelular.Ficha(SJogador p, bool nota, bool amarelos, ...)`, `MercadoRetro.Ficha(SJogador p, bool nota, bool amarelos, IBrush fundo, IBrush cor, string? valorMercado = null)`, `LeilaoCelular(..., bool nota, bool amarelos)` e `TelaLeilao.Ficha(..., bool nota, bool amarelos)`.

Regra de classificação para cada uso de `_launcher`/`launcher`/`Regras.CartoesAmarelos` nestas telas:
- Nota, RL, colunas "Nota", filtros e ordem por Nota/RL e `_mais` do Procurar → `NotaERisco`.
- "A", "N amarelos acumulados" e a linha "Cartões amarelos" do histórico → `CartoesAmarelos`.
- Ingresso, campinho e árbitro → Task 5 (não mexer aqui).

- [ ] **Step 1: Trocar as assinaturas compartilhadas** (Interfaces) e todos os chamadores.
- [ ] **Step 2: Separar as flags nas telas.** Onde uma tela tem um único `_launcher`, criar `_nota` e `_amarelos` e reclassificar cada uso pela regra acima. As posições que mudavam com `_launcher` passam a depender só da flag do recurso que ocupa aquele espaço. Ex.: `TelaEquipa` 1439, `ys/yl`, depende de existir a linha Nota/Risco acima → `_nota`. `TelaLeilao` 590, `passo`, → `_nota`.
- [ ] **Step 3: Procurar (Review Focus 5).** Com `_nota` falso, `Ordens` tem 2 itens a menos e uma ordem escolhida ≥ esse tamanho volta ao nome (o código de 227 já faz isso; confirme que vale também para a troca da opção entre aberturas). Os filtros de Nota/RL somem e voltam a "Qualquer".
- [ ] **Step 4: Compilar.** `dotnet build RetroFoot.Desktop`. Esperado: 0 erros. Depois `grep -n "CartoesAmarelos" RetroFoot.App/*.cs`: cada ocorrência que sobrar deve tratar só dos amarelos, ou ser das Configurações e dos resumos.

### Task 5: Preço do ingresso, campinho e árbitro (tela do time)

**Files:**
- Modify: `RetroFoot.App/TelaEquipa.cs` (Finanças 1478-1530, 1917-1921, 1985-1988; campinho 1574; árbitro 1639, 1694-1700)
- Modify: `RetroFoot.App/TelaEquipaModerna.cs:902-967,1106-1119`
- Modify: `RetroFoot.App/TelaEquipaCelular.cs:346,426-486` (bloco do ingresso e árbitro)

**Interfaces:**
- Consumes: `Regras.PrecoIngresso`, `Regras.CampinhoArbitro`.
- Produces: em `TelaEquipa` (Painel), `_ingresso` e `_campinho` (`readonly bool`). `ArbitroCompleto => _campinho || Registo.AddIn(0x23)`.

- [ ] **Step 1: Ingresso.** `yt/yj/yc` e o preço fixo em 1490 dependem de `_ingresso`; o clique em `RIngressos`, a roda (1985) e a previsão também. No Moderno, `cartaoIngresso = _ingresso && !duas`. No celular, o bloco do preço e a previsão só aparecem com `PrecoIngresso`.
- [ ] **Step 2: Campinho e árbitro.** Campinho (1574) e o tamanho da fonte do texto do árbitro (1700) dependem de `_campinho`; `ArbitroCompleto` conforme Interfaces (Clássico, Moderno e celular). A frase "Pênalti/Cartões" fica inteira.
- [ ] **Step 3: Compilar e conferir que não sobrou `_launcher`.** `dotnet build RetroFoot.Desktop`. Depois `grep -n "_launcher" RetroFoot.App/TelaEquipa*.cs`: só podem sobrar comentários.

### Task 6: Capturas, conferência visual e CHANGELOG

**Files:**
- Modify: `ferramentas/FichasPrint/Program.cs` (novos modos e flags)
- Modify: `CHANGELOG.md` ([Não lançado] → Novidades)

**Interfaces:**
- Consumes: tudo das Tasks 1–5.

- [ ] **Step 1: Ampliar o FichasPrint.** Novos modos: `equipa-jogador`, `equipa-financas`, `equipa-arbitro`, `procura` e `config-regras`, além de `plantel` e `leilao`. As flags vêm de variáveis de ambiente que sobrescrevem a config lida: `RETROFOOT_AMARELOS`, `RETROFOOT_NOTA`, `RETROFOOT_INGRESSO` e `RETROFOOT_CAMPINHO`, cada uma `0` ou `1`, aplicadas em `Modificada = true` antes de `OpcoesDoJogo`. O formato vem de `RETROFOOT_FORMATO=43|169` (via `Formato.Aplicar`). O nome do arquivo inclui modo, visual, formato e combinação.
- [ ] **Step 2: Gerar a matriz** (no máximo ~6 processos em paralelo), com saída na pasta de rascunho da sessão:
  - visuais: `classico`, `moderno` com `RETROFOOT_TEMA` = `verde`, `escuro` e `claro`, celular Moderno e celular Retrô (`RETROFOOT_CELULAR=1`);
  - formatos: 4:3 e 16:9 (só no computador);
  - combinações: todas ligadas; todas desligadas; só Nota; só Ingresso; amarelos sem ingresso; ingresso sem amarelos; só Campinho;
  - modos: os seis do Step 1.
- [ ] **Step 3: Conferir as imagens.** Nada sobreposto, cortado ou fora da janela, e cada recurso aparece se e só se a sua flag está ligada. Corrigir e gerar de novo o que falhar.
- [ ] **Step 4: Soak rápido.** Um SoakUI na Atual com tudo ligado, e outro com `SEM_AMARELOS=1 SEM_CAMPINHO=1`, 1 temporada cada. Esperado: código 0, sem "EXCECAO".
- [ ] **Step 5: CHANGELOG.** Em [Não lançado] → Novidades: "Configurações: as opções que vinham junto com os cartões amarelos agora são separadas: Nota e risco de lesão, Preço do ingresso e Campinho e árbitro, agrupadas pelo que afetam." Em Correções: "Desligar os cartões amarelos não esconde mais a nota, o preço do ingresso, o campinho e o árbitro."
- [ ] **Step 6: Suíte final.** `dotnet test RetroFoot.Motor.Testes` e `dotnet build RetroFoot.Desktop`. Esperado: tudo passa e 0 erros. Deixar sem commit.
