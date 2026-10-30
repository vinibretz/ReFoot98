# Premiação ampliada — plano de implementação

> **Para agentes:** SUB-SKILL OBRIGATÓRIA: use superpowers:subagent-driven-development ou superpowers:executing-plans para executar este plano tarefa a tarefa. Os passos usam checkbox (`- [ ]`).

**Objetivo:** opção "Premiação: Clássica / Ampliada" (versão Modificada, gravada no save) que paga prêmios proporcionais às K primeiras de cada divisão (1ª a 4ª) e ao vice da copa.

**Arquitetura:** cálculo puro em `RetroFoot.Motor/Temporada/PremiacaoAmpliada.cs`; a flag vive em `Regras` (com marca no save, como o K); `Epoca.FimDeTemporada` captura a classificação antes da subida/descida, paga e devolve a lista em `Resultado.Premios`; a App lê `Resultado.Premios` nas telas e expõe a opção nas Configurações.

**Stack:** C# / .NET 8, Avalonia, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-29-premiacao-ampliada-design.md`

## Restrições globais

- Fatores por divisão sobre `P = Moeda.PremioCampeao`: 1ª 100%, 2ª 40%, 3ª 15%, 4ª 5%; Distrital nada.
- Posição `i` (1 = primeira) das K primeiras: fator `(K − i + 1) / K`; K = `Regras.SobemDescem`.
- Valor = `P × fator divisão × fator posição`, arredondado ao múltiplo de 1000 mais próximo (meio para cima). Campeão da 1ª = exatamente `P`.
- Vice da copa = `Moeda.PremioTaca × 50%`, arredondado igual.
- Sem inflação. Pagamento com `MercadoJogo.Movimento(e, valor, MercadoJogo.TipoPremios)`.
- Com a Clássica o comportamento e os bytes do save ficam idênticos aos de hoje; nenhuma chamada nova ao `Random`.
- Opção só na versão Modificada; na Original é sempre Clássica.
- **Preferência do usuário:** não commitar. Cada tarefa termina com as mudanças no working tree.
- Base de testes nesta máquina: 137 aprovados / 78 falhas (dependem do jogo original). Nenhum dos 137 pode passar a falhar.

## Foco de revisão

1. Save do original/automático sem a marca → abre como Clássica (byte 19 do nome do Distrital ≠ 0xA5). Teste na Tarefa 2.
2. Nome do Distrital longo (≥ 19 caracteres) → não marca e continua Clássica, sem corromper o nome. Teste na Tarefa 2.
3. Copa sem final jogada (nenhuma entrada de copa jogada no calendário do campeão) → sem vice, sem exceção. Teste na Tarefa 3.
4. Mesma equipa em dois prêmios (campeã da 1ª e vice da copa) → recebe os dois. Teste na Tarefa 3.
5. Classificação usada é a de antes de `SubidaEDescida` (as que sobem da 2ª recebem como 2ª, não como 1ª). Teste na Tarefa 3.

---

### Tarefa 1: cálculo dos valores

**Arquivos:**
- Criar: `RetroFoot.Motor/Temporada/PremiacaoAmpliada.cs`
- Teste: `RetroFoot.Motor.Testes/PremiacaoTestes.cs`

**Interfaces — produz:**
```csharp
namespace RetroFoot.Motor.Temporada;
public static class PremiacaoAmpliada
{
    public static readonly double[] FatorDivisao = { 1.0, 0.40, 0.15, 0.05 };
    // divisao 0..3, posicao 1..k; fora disso, 0
    public static int Valor(int premioCampeao, int divisao, int posicao, int k);
    public static int ViceTaca(int premioTaca);
}
// em Epoca.cs, junto de Resultado
public sealed record Premio(int Divisao, int Posicao, ushort Equipa, int Valor);   // Divisao -1 = copa (Posicao 2)
```

- [ ] **Passo 1: testes que falham** em `PremiacaoTestes`:
  - `Valor(20_000_000, 0, 1, 2) == 20_000_000`, `(…,0,2,2) == 10_000_000`, `(…,1,1,2) == 8_000_000`, `(…,2,2,2) == 1_500_000`, `(…,3,1,2) == 1_000_000`, `(…,3,2,2) == 500_000`.
  - K = 3: `Valor(20_000_000, 0, 2, 3) == 13_333_000`, `(…,0,3,3) == 6_667_000`.
  - K = 4: `Valor(20_000_000, 1, 4, 4) == 2_000_000`.
  - Fora da faixa: `Valor(P, 4, 1, 2) == 0`, `Valor(P, 0, 3, 2) == 0`, `Valor(P, 0, 0, 2) == 0`.
  - `ViceTaca(6_000_000) == 3_000_000`, `ViceTaca(5_000_000) == 2_500_000`.
- [ ] **Passo 2:** `dotnet test RetroFoot.Motor.Testes --filter FullyQualifiedName~PremiacaoTestes` → falha de compilação.
- [ ] **Passo 3:** implementar (aritmética em `decimal` ou `long`, arredondamento `MidpointRounding.AwayFromZero` ao milhar). `Premio` declarado em `Epoca.cs`.
- [ ] **Passo 4:** mesmo filtro → todos passam.

### Tarefa 2: flag em `Regras` e marca no save

**Arquivos:**
- Modificar: `RetroFoot.Motor/Regras/Regras.cs` (perto de `SobemDescemEscolhido`, `DoSave`, `MarcarSobemDescem`)
- Modificar: `RetroFoot.Motor/NovoJogo/Exportar.cs:45-47`
- Teste: `RetroFoot.Motor.Testes/PremiacaoTestes.cs`, e helper `RetroFoot.Motor.Testes/DadosAbertos.cs`

**Interfaces — produz:**
```csharp
public bool PremiacaoAmpliada { get; init; }                // em Regras
public static bool PremiacaoDoSave(Jogo98 s);               // Divisoes[4].Nome[19] == 0xA5
public static bool MarcarPremiacao(Jogo98 s, bool ampliada); // false se Divisoes.Count <= 4 ou Nome[0] >= 19
// DoSave passa a fazer também: PremiacaoAmpliada = PremiacaoDoSave(s)
// Exportar: if (e.Regras.PremiacaoAmpliada) Regras.MarcarPremiacao(s, true)
```
```csharp
// testes: a pasta de dados aberta do jogo (sem depender do original)
static class DadosAbertos
{
    public static string Pasta();   // sobe de AppContext.BaseDirectory até achar RetroFoot.App/Assets/Dados
    public static Jogo98 JogoNovo(Regras r, uint semente = 1); // como SobemDescemTestes.JogoNovo.Criar, com Dados.Ler(Pasta()), Moeda Reais
}
```
Constantes: `IndiceMarcaPremiacao = 19`, `MarcaPremiacao = 0xA5`. Comentário no estilo do K explicando o byte.

- [ ] **Passo 1: testes que falham**:
  - `JogoNovoClassicoSemMarca`: `JogoNovo(Launcher(8))` → `PremiacaoDoSave == false`, `DoSave(s, Original).PremiacaoAmpliada == false`.
  - `JogoNovoAmpliadoComMarca`: `JogoNovo(Launcher(8) with { PremiacaoAmpliada = true })` → `PremiacaoDoSave(Jogo98.Ler(s.Gravar())) == true`; depois `MarcarPremiacao(s, false)` → bytes iguais aos do jogo Clássico com a mesma semente.
  - `MarcaNaoMexeNoK`: com `SobemDescemEscolhido = 3` e Ampliada, `SobemDescemDoSave == 3` e `PremiacaoDoSave == true`.
  - `NomeLongoNaoMarca`: Distrital com `Nome[0] = 19` → `MarcarPremiacao` devolve false e `Nome` não muda.
- [ ] **Passo 2:** rodar → falham.
- [ ] **Passo 3:** implementar.
- [ ] **Passo 4:** `dotnet test RetroFoot.Motor.Testes` → novos passam; contagem antiga mantém 137 aprovados entre os antigos.

### Tarefa 3: pagar no fim da temporada

**Arquivos:**
- Modificar: `RetroFoot.Motor/Temporada/Epoca.cs` (`Resultado`, `FimDeTemporada` ~139-188, `Premios` ~245)
- Teste: `RetroFoot.Motor.Testes/PremiacaoTestes.cs`

**Interfaces — consome:** Tarefas 1 e 2. **Produz:** `Resultado.Premios` (`List<Premio>`, vazia na Clássica), ordem: divisão 0..3 e posição crescente, depois a copa.

Em `FimDeTemporada`, antes de `SubidaEDescida(r)`, se `Regras.PremiacaoAmpliada`: guardar as K primeiras ids de `S.Divisoes[0..3]` e o vice da copa = `W08` da última entrada do calendário de `taca` com `B07 == 1` e `W0A != 0xFFFF` (jogada); nenhuma → sem vice. `Premios(camp, taca)` continua pagando campeão/copa/artilheiro como hoje; com a Ampliada paga, além disso, as posições 2..K da 1ª (o campeão já recebe `P` no caminho original — não pagar duas vezes), todas as K da 2ª à 4ª, e o vice, preenchendo `r.Premios` (incluindo a linha do campeão da 1ª com `P`, para as telas).

- [ ] **Passo 1: testes que falham** (jogo de `DadosAbertos`, N = 8, K = 2, Reais; `S.OutrasEquipas` reduzido a uma equipa campeã da copa com uma entrada de copa jogada contra o vice):
  - `ClassicaNaoMuda`: Clássica → `r.Premios` vazia; dinheiro do 2º da 1ª e do 1º da 2ª inalterados.
  - `AmpliadaPagaAsDivisoes`: dinheiro depois − antes = 10 mi para o 2º da 1ª, 8 mi / 4 mi para 1º/2º da 2ª, 3 mi / 1,5 mi na 3ª, 1 mi / 0,5 mi na 4ª (usar equipas que não são campeã/vice da copa nem do artilheiro, ou descontar esses); `r.Premios.Count == 9` (8 de divisões + vice).
  - `CampeaoDaPrimeiraNaoRecebeEmDobro`: o campeão da 1ª recebe `P` uma vez (mais copa/artilheiro se for o caso).
  - `ViceDaCopa`: recebe 3 mi; `Premio(-1, 2, vice, 3_000_000)` na lista.
  - `SemFinalSemVice`: sem entrada de copa jogada → nenhum `Premio` com `Divisao == -1`, sem exceção.
  - `AcumulaPremios`: vice da copa = 2º da 1ª → recebe 10 mi + 3 mi.
  - `UsaClassificacaoDeAntes`: os ids em `r.Premios` com `Divisao == 1` são `antes[1].Take(2)` (mesmo que depois da troca estejam na 1ª).
  - `BalancoPremios`: a linha "premios" de `MercadoJogo.Balanco(e)` do 1º da 2ª soma 8 mi.
- [ ] **Passo 2:** rodar → falham.
- [ ] **Passo 3:** implementar.
- [ ] **Passo 4:** `dotnet test RetroFoot.Motor.Testes` → novos passam; os 137 antigos continuam.
- [ ] **Passo 5:** em `SoakTestes`, um `[Fact] CurtoPremiacao` como `CurtoSobemDescem` com `Mod(16) with { PremiacaoAmpliada = true }`, 2 temporadas; roda com `RETROFOOT_JOGO=<repo>/RetroFoot.App/Assets/Dados` → sem problemas.

### Tarefa 4: opção nas Configurações

**Arquivos:**
- Modificar: `RetroFoot.App/Configuracoes.cs` (propriedade, `RegrasPara` ~79, `OpcoesDoJogo` ~94, leitura ~201, gravação ~226)
- Modificar: `RetroFoot.App/TelaConfiguracoes.cs` (moderno ~170/225/360/384; retrô ~568/610/638; resumos ~144 e ~421)
- Modificar: `RetroFoot.App/TelaConfiguracoesCelular.cs` (~93, ~158-162), `TelaConfiguracoesCelularRetro.cs` (~92-98), `TelaInicioModerna.cs:180`
- Criar: `RetroFoot.App/Assets/Idiomas/{pt-BR,pt-PT,en,es}/premiacao.txt` (formato `chave<TAB>tradução`, cabeçalho como `sobe-desce.txt`)

**Interfaces — produz:** `Configuracoes.PremiacaoAmpliada` (bool), chave `config.json` `"premiacao"` (0 = Clássica, 1 = Ampliada; ausente = 0). `RegrasPara` passa `PremiacaoAmpliada = PremiacaoAmpliada` no ramo Modificada. `OpcoesDoJogo`: `mod` também quando `doSave.PremiacaoAmpliada`.

Em cada tela, um bloco "Premiação" logo abaixo de "Sobem e descem por divisão", com o mesmo controle e a mesma regra de habilitação (só com `_cfg.Modificada`), opções `T("Clássica")` / `T("Ampliada")`, e ajuda fixa:
`T("Ampliada: além do campeão, da copa e do artilheiro, premia as que sobem de cada divisão e o vice da 1ª e da copa, proporcional à divisão e à posição. Fica gravado no jogo novo e não muda depois.")`
Resumos: acrescentar `T("premiação ampliada")` quando ligada.

- [ ] **Passo 1:** implementar `Configuracoes` + teste não há para a App; validar com `dotnet build RetroFoot.Desktop` → 0 erros.
- [ ] **Passo 2:** telas e traduções (pt-BR: "times" em vez de "equipas", como em `sobe-desce.txt`).
- [ ] **Passo 3:** `dotnet run --project RetroFoot.Desktop`: nos visuais Retrô e Moderno, a opção aparece, fica desabilitada na Original, persiste ao reabrir.

### Tarefa 5: telas de fim de temporada

**Arquivos:**
- Modificar: `RetroFoot.App/TelaFimDeTemporada.cs` (`Mostrar` ~34, `Janela` ~40-121, `JanelaModerna` ~138, nova `PremiacaoDivisoes`)
- Modificar: `RetroFoot.App/FimDeTemporadaCelular.cs` (bloco de prêmios ~199-201, nova `PremiacaoDivisoes`)
- Traduções em `premiacao.txt`

**Interfaces — consome:** `Resultado.Premios`.

- Linha do **vice da copa** no bloco "Prêmios" das três janelas, só quando há `Premio` com `Divisao == -1`: Retrô em y = 272 (rótulo x 312; valor x 448 com largura reduzida para não invadir o botão em 540,280); Moderno e Celular, mais uma linha na lista de prêmios. Texto `T("Vice da copa")`.
- `internal static Control PremiacaoDivisoes(Sessao s, Resultado r, Action fechar)`: Retrô no padrão de `SubidasEDescidas` (cabeçalho por divisão `T("{0}ª Divisão", d + 1)`, linhas `"{posição}º  {NomeCurto}  {valor}"`), título `T("Premiação das divisões")`; Moderno no padrão de `SubidasModerna`; Celular no padrão de `FimDeTemporadaCelular.SubidasEDescidas`.
- `Mostrar`: depois da janela principal (e antes da de subidas), `if (r.Premios.Any(p => p.Divisao >= 0)) s.EsperarDialogo<bool>(fim => PremiacaoDivisoes(s, r, () => fim(true)));`

- [ ] **Passo 1:** implementar; `dotnet build RetroFoot.Desktop` → 0 erros.
- [ ] **Passo 2:** acrescentar o caso em `DemoDialogos.cs` / `DemoCelularListas.cs` como já existe para `SubidasEDescidas`, para conferir sem jogar uma temporada.
- [ ] **Passo 3:** verificação manual pelo usuário (`dotnet run`): jogo novo Modificada + Ampliada, ir ao fim da temporada, conferir as duas janelas e o Balanço.

### Tarefa 6: documentação

- Criar `docs/premiacao.md` no estilo de `docs/sobem-descem.md` (regra, tabela de exemplo Reais N = 8, marca no save: byte 19 do nome do Distrital = 0xA5, testes).
- README, seção Destaques: um item "Premiação ampliada (opcional)".
- Seção no README ou em `docs/` dizendo que é um fork de juan-silveira/RetroFoot (GPL v3) com as mudanças listadas.
- [ ] Revisar e deixar sem commit.
