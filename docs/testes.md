# Testes

Antes de lançar uma versão, rode na raiz do repositório:

    pwsh -File .\scripts\testar-tudo.ps1

O script leva uns 10 minutos e grava tudo em `dist\testes\<data>\`:

1. **Testes do motor** (`RetroFoot.Motor.Testes`): as regras, os saves, o online no banco da
   memória. Os que precisam do jogo original de 1998 aparecem como ignorados quando os arquivos
   não existem (ver abaixo).
2. **Testes da interface** (`RetroFoot.App.Testes`): as telas de verdade numa janela sem tela,
   em 8 visuais (Clássico 4:3 e 16:9, Moderno 4:3 e 16:9 nos temas verde, escuro e claro,
   celular Moderno e Retrô).
3. **Soaks**, três em paralelo: uma temporada do jogo com as opções da versão Atual ligadas,
   uma com elas desligadas e 3 jornadas do online (host e convidado). Um soak que trava ou lança exceção
   falha.
4. **Capturas** de cada tela, em cada visual, com tudo ligado e tudo desligado, numa página
   (`index.html`) para passar o olho. É a parte que continua com você: o teste vê texto por cima
   de texto e texto cortado, mas não vê se ficou bonito.

O resumo sai na tela e em `resumo.txt`. O script termina com código 1 se algum passo falhou.
`-SemSoaks` e `-SemCapturas` pulam esses passos (só os testes: uns 2 minutos).

O que continua manual: o Android no aparelho (instalar o APK e abrir as telas) e uma partida
online de verdade entre dois aparelhos (o automático usa o banco da memória, não o Firebase).

## Os testes da interface

Cada teste abre a tela (`Cena`: um jogo novo de 8 equipas criado de `RetroFoot.App/Assets/Dados`,
a mesma `Sessao` do jogo) e lê de volta os textos desenhados (`Inspecao.Textos`): os dos
controles e os das telas desenhadas à mão (o Clássico, as fichas), com a posição na janela, o
recorte e se estão cobertos por um fundo opaco desenhado depois (a página do celular por cima da
anterior, um diálogo por cima da tela).

- `LayoutTestes`: em cada visual, as telas das opções (Configurações, ficha do Elenco, Leilão,
  Procurar, tela do time no jogador, nas Finanças e no árbitro) com tudo ligado e tudo desligado,
  nos 4 idiomas: nenhum texto por cima de outro e nenhum cortado na largura.
- `OpcoesTestes`: cada opção da versão Atual (Cartões amarelos, Nota e risco de lesão, Preço do
  ingresso, Campinho e árbitro) mostra ligada e esconde desligada só o que é dela, em cada visual.
- `ConfiguracoesTestes`: os grupos da página Regras, cada interruptor mudando só a sua opção, a
  versão Retrô travada (o aviso, as quatro desligadas na tela, a escolha guardada), o resumo do
  Moderno 16:9, o `config.json` (gravar e ler; o de antes das opções separadas herda os
  amarelos) e as regras do jogo seguindo as opções.

Os testes não tocam nas Configurações nem nos saves de verdade: a pasta de dados é a da saída
dos testes, começando sem `config.json`.

### Escrever um teste novo

Para ver o que uma tela desenha (e o que o teste vai achar), o `Despejo`:

    $env:DESPEJO = "C:\pasta"; $env:DESPEJO_TELA = "financas"
    dotnet test RetroFoot.App.Testes --filter Despejar

Um arquivo por visual e combinação, com cada texto (`x` = fora da vista), as sobreposições e os
cortes. As telas novas entram em `Cena` (como abrir) e em `LayoutTestes.Telas`. Os ganchos das
telas para abrir numa seção ou página são variáveis de ambiente lidas no construtor
(`RETROFOOT_SECAO` nas Configurações, `RETROFOOT_PAGINA` na tela do time, também no celular), os
mesmos do `ferramentas/FichasPrint`.

## Sem o jogo original

Cerca de 23 testes do motor precisam de arquivos do jogo de 1998 que não vão para o git: a pasta
do jogo (`RETROFOOT_JOGO`), os casos do oráculo (`RetroFoot.Motor.Testes/Oraculo/*-original.e98`)
e os saves do launcher (`RetroFoot.Motor.Testes/Patrocinio/`). Sem eles, aparecem como ignorados
(`RetroFoot.Motor.Testes/Requisitos.cs`: `[FactComOriginal]`, `[FactComArquivo]`,
`[TheoryDoOraculo]`). Um teste novo que precise deles usa esses atributos.

Os testes que leem saves reais usam `RetroFoot.Motor.Testes/saves-locais.txt` (fora do git), que
aponta para `RetroFoot.Motor.Testes/SavesLocais` (saves do jogo e do SoakUI com `GUARDAR_SAVE`).
Ao trocar esses saves, registre em `AcrescimosTestes` a impressão nova que
`SemAcrescimosIgualAoOriginal` mostra.
