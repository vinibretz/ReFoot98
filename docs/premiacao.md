# Premiação ampliada

Opção da versão Atual (Modificada) (`Regras.PremiacaoAmpliada`; Configurações > Regras >
"Premiação ampliada", chave `premiacaoAmpliada` do `config.json`). Desligada (padrão) é a
**Clássica**: só os três prêmios do 98.003 (campeão, copa e artilheiro). Na versão Retrô é
sempre a Clássica.

## A regra

Com `P` = prêmio do campeão da moeda (`Moeda.PremioCampeao`), `T` = prêmio da copa
(`Moeda.PremioTaca`) e `K` = quantas sobem e descem (`Regras.SobemDescem`, automático ou
escolhido):

- As **K primeiras** da classificação final de cada divisão, da 1ª à 4ª, recebem
  `P × fator da divisão × (K − i + 1) / K` (i = posição, 1 = primeira), arredondado ao
  milhar. Na 1ª são o campeão e as K − 1 seguintes; da 2ª à 4ª, as que sobem.
- Fator da divisão: 1ª 100%, 2ª 40%, 3ª 15%, 4ª 5%. O Distrital não premia (sobe por
  sorteio).
- O **vice da copa** (o derrotado na final) recebe `T / 2`.
- Campeão da 1ª, vencedor da copa e artilheiro continuam como no original. Uma equipa que
  ganha mais de um prêmio recebe todos.
- Sem inflação, como os prêmios originais. Entra na linha "Prêmios" do Balanço.

Os fatores seguem a escada de faturamento das divisões (bilheteria média por equipa numa
temporada simulada, em Reais, N = 8: 18,5 / 11,3 / 4,3 / 1,0 milhões).

Exemplo, Reais, 8 equipas, K = 2:

| | 1º | 2º |
|---|---|---|
| 1ª divisão | 20 mi | 10 mi |
| 2ª divisão | 8 mi | 4 mi |
| 3ª divisão | 3 mi | 1,5 mi |
| 4ª divisão | 1 mi | 0,5 mi |
| Copa | 6 mi | 3 mi |

## Onde fica no código

- Cálculo: `RetroFoot.Motor/Temporada/PremiacaoAmpliada.cs`.
- Pagamento: `Epoca.FimDeTemporada` guarda as K primeiras de cada divisão e o vice da copa
  **antes** da subida e descida e paga depois dos prêmios do original
  (`Epoca.PremiosAmpliados`); a lista vai em `Resultado.Premios`.
- O vice da copa é o adversário do campeão na última entrada **jogada** da Taça no
  calendário dele (+0x07 = 1, golos ≠ 0xFFFF): o save não muda de formato.
- Telas: a linha "Vice da copa" na janela de Fim de Temporada e, depois dela, a janela
  "Premiação das divisões" (`TelaFimDeTemporada.PremiacaoDivisoes`), nos visuais Retrô,
  Moderno e no celular.

## No save

A escolha fica no jogo (não muda se as Configurações mudarem depois), sem mudar o formato
do `.e98`: vai no 20º byte (índice 19) do nome do Distrital, um `string[20]` do Pascal
(`Divisao.Nome` da divisão 4), logo antes da marca do K (índice 20,
[sobem-descem.md](sobem-descem.md)). Valor `0xA5` = Ampliada; qualquer outro = Clássica.
Com um nome do Distrital de 19 ou mais caracteres não há onde marcar e o jogo fica
Clássico. `Regras.PremiacaoDoSave` / `MarcarPremiacao`; um save com a marca abre sempre
como Modificada.

Testes: `PremiacaoTestes` (valores, marca no save, fim de temporada com jogo novo dos dados
abertos) e `SoakTestes.CurtoPremiacao` (duas temporadas com recargas; precisa de
`RETROFOOT_JOGO` apontando para `RetroFoot.App/Assets/Dados` ou para o jogo original).
