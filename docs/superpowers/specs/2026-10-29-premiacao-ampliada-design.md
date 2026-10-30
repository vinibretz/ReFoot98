# Premiação ampliada — design

## Intenção

Hoje o fim de temporada paga só três prêmios ([Epoca.Premios](../../../RetroFoot.Motor/Temporada/Epoca.cs)):
campeão da 1ª divisão, vencedor da copa e artilheiro (valores fixos da moeda, sem
inflação; em Reais 20 mi / 6 mi / 3 mi). Quem é campeão ou sobe nas divisões de baixo
não ganha nada, nem o vice da 1ª nem o vice da copa.

A proposta é uma opção de economia, **Premiação ampliada**, que premia de forma
proporcional as posições de destaque de todas as divisões jogadas e o vice da copa,
respeitando o número de equipas por divisão (N) e quantas sobem e descem (K). O objetivo
é dar fôlego a quem sobe (humano ou CPU) sem desequilibrar a economia da 1ª divisão.

## Referência econômica (medida)

Uma temporada simulada (Soak, semente 3, Reais), média por equipa:

| Divisão | Bilhetes (N=8) | Bilhetes (N=16) | Salários (N=8) |
|---|---|---|---|
| 1ª | 18,5 mi | 32,3 mi | −7,8 mi |
| 2ª | 11,3 mi | 19,4 mi | −6,0 mi |
| 3ª | 4,3 mi | 7,2 mi | −3,2 mi |
| 4ª | 1,0 mi | 1,9 mi | −1,1 mi |

Os fatores por divisão abaixo seguem aproximadamente essa escada de faturamento.

## Regras da premiação ampliada

Seja `P` = `Moeda.PremioCampeao`, `T` = `Moeda.PremioTaca` e `K` = `Regras.SobemDescem`
(o K efetivo do jogo, automático ou escolhido, já lido do save).

**Fator da divisão** (sobre `P`):

| Divisão | Fator |
|---|---|
| 1ª | 100% |
| 2ª | 40% |
| 3ª | 15% |
| 4ª | 5% |
| 5ª (Distrital) | não premia (a subida é por sorteio, não há classificação) |

**Posições premiadas**: as K primeiras da classificação final de cada uma das divisões
1 a 4. Na 1ª são o campeão e os K − 1 seguintes (com K = 2: campeão e vice); da 2ª à 4ª
coincidem com as que sobem.

**Fator da posição** (linear decrescente): a posição `i` (1 = campeão) recebe
`(K − i + 1) / K`.

- K = 1: 100%
- K = 2: 100%, 50%
- K = 3: 100%, 67%, 33%
- K = 4: 100%, 75%, 50%, 25%

**Valor** = `P × fator da divisão × fator da posição`, arredondado para o múltiplo de
1000 mais próximo. O campeão da 1ª continua recebendo exatamente `P` (como hoje).

**Copa**: o vencedor continua com `T`; o **vice da copa** (o perdedor da final) recebe
`T × 50%`.

**Artilheiro**: inalterado.

**Sem inflação**, como os prêmios originais. Prêmios são pagos para equipas humanas e
da CPU igualmente, com `MercadoJogo.Movimento(..., TipoPremios)`, então entram na linha
"Prêmios" do Balanço sem mudar nada no formato do save.

Exemplo, Reais, N = 8, K = 2 (automático):

| | Campeão | 2º |
|---|---|---|
| 1ª | 20 mi | 10 mi |
| 2ª | 8 mi | 4 mi |
| 3ª | 3 mi | 1,5 mi |
| 4ª | 1 mi | 0,5 mi |
| Copa | 6 mi | 3 mi |

Total pago por temporada sobe de ~29 mi para ~60 mi (com artilheiro). No conjunto da liga
é pouco (~6% do faturamento), mas nas divisões de baixo o campeão recebe algo como uma
temporada inteira de faturamento: esse é o efeito desejado.

## A opção

- Nome na interface: **Premiação** — "Clássica" (padrão) / "Ampliada". Fica em
  Configurações > Regras, junto de "Sobem e descem por divisão", e **só existe na versão
  Modificada** (na Original é sempre a Clássica), seguindo o padrão das outras opções
  modificadas (`Configuracoes.cs`, chave nova no `config.json`, ex. `premiacao`: 0/1).
- Vira um campo de `Regras` (ex. `PremiacaoAmpliada`), lido pelo motor no fim de temporada.
- **Fica gravada no save**, como o K (`Regras.MarcarSobemDescem` / `SobemDescemDoSave`):
  o jogo não muda de economia se as Configurações mudarem depois; `Regras.DoSave` lê do
  save. Um save com a marca abre como Modificada. **Restrição:** sem mudar o formato do
  `.e98` e sem afetar saves do original (marca ausente = Clássica). O byte exato a usar
  fica com o construtor; precisa ser um byte comprovadamente não lido pelo jogo e que não
  colida com a marca do K (ver docs/sobem-descem.md para o método usado lá).
- Clássica = comportamento atual byte a byte (o oráculo e os testes existentes não podem
  mudar).

## Onde aparece

- **Fim de temporada** (telas `TelaFimDeTemporada.cs` e `FimDeTemporadaCelular.cs`, nos
  dois visuais): com a Ampliada, além dos três prêmios atuais, mostrar o vice da copa e os
  premiados de cada divisão (equipa e valor). Com a Clássica, nada muda.
  - **Visual Retrô (decidido)**: a janela original (`TEndOfSeasonDlg`) continua fiel ao
    98.003, só ganhando a linha do vice da copa (se não couber no layout fixo, o construtor
    ajusta o mínimo possível). Em seguida abre uma **segunda janela**, "Premiação das
    divisões", no estilo da lista de subidas e descidas (`TPrintListDlg`), com divisão,
    posição, equipa e valor, fechando no OK. Aparece sempre que a Ampliada está ligada.
  - **Visual Moderno e Celular**: a tela rola; o layout fica com o construtor.
- **Resumo das regras** nas Configurações e no jogo novo, como o K já aparece.
- **Balanço**: automático, via a linha "Prêmios".
- Textos nos 4 idiomas (pt-BR, pt-PT, en, es) — ver docs/glossario-idiomas.md.
- Atualizar o README (Destaques) e criar `docs/premiacao.md` no estilo de
  docs/sobem-descem.md.

## Pontos de atenção para o construtor

- **Ordem no fim de temporada**: `Premios` roda depois de `SubidaEDescida`, que troca
  equipas de divisão. A classificação final (as K primeiras de cada divisão) precisa ser
  capturada **antes** das trocas, como já é feito com `camp` e `taca`.
- **Vice da copa**: o perdedor é tirado de `S.OutrasEquipas` (`FimDeJogo.TirarDaTaca`)
  quando perde, então no fim da temporada só sobra o campeão. É preciso identificar o
  finalista derrotado (no momento da final ou pelo histórico do jogo) sem mudar o formato
  do save; se precisar persistir entre recargas (o jogo pode ser salvo entre a final e o
  fim da temporada), resolver isso dentro da mesma restrição.
- Se a mesma equipa ganhar mais de um prêmio (ex. campeã da 1ª e vice da copa), recebe
  todos.
- Patrocínios (`AntesDosPremios`) ficam como estão.
- Nenhuma chamada nova ao `Random`: a sequência aleatória do jogo não pode mudar.

## Testes esperados

- Com a Clássica: os testes atuais continuam passando (137 hoje nesta máquina; os 78 que
  falham dependem dos arquivos do jogo original e não contam).
- Com a Ampliada: valores por divisão e posição para N = 8 (K = 1 a 3; com 8 equipas o
  máximo é 3) e N = 16 (K = 1 a 4); o vice da
  copa; a classificação usada é a de antes da subida/descida; a marca no save (gravar, ler,
  ausente = Clássica); um Soak curto com a opção ligada.

## Fora do escopo

- Mexer nos valores originais (campeão, copa, artilheiro).
- Prêmios por jogo/vitória, bônus de bilheteria, inflação nos prêmios.
- Premiar a 5ª divisão.
- Configurar os percentuais pela interface (ficam fixos no código; podem virar opção
  depois).
