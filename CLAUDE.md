# ReFoot98 (fork vinibretz do RetroFoot)

## Registro de novidades (obrigatório)

- Toda mudança feita no projeto entra no `CHANGELOG.md`, na seção **[Não lançado]**, na mesma
  sessão em que foi implementada (antes de encerrar a tarefa, mesmo sem commit). Assim as
  mudanças de várias sessões se acumulam até a próxima versão.
- Use as subseções **Novidades**, **Correções** e **Interno**; uma linha por item, escrita
  para o jogador (Interno: testes, docs, ferramentas). Prefixe a área quando ajudar
  ("Online:", "Android:", "Moderno:").
- Antes de escrever, leia a seção: se o item já existe (de outra sessão), atualize-o em vez
  de duplicar; se uma mudança desfaz outra ainda não lançada, ajuste ou remova a linha antiga.
- Ao lançar uma versão, siga `docs/release.md`: a seção [Não lançado] vira a da versão e é a
  base do commit, da tag e da descrição da release.

## Release

- Rode `scripts/gerar-release.ps1` sempre com o PowerShell 7: `pwsh -File .\scripts\gerar-release.ps1 -Versao X.Y.Z`.
  Com o Windows PowerShell antigo (`powershell.exe` 5.1) falta o `TarWriter`: o `.tar.gz` do
  Linux sai com 0 bytes e o APK não é gerado, mas o script termina com código 0. Depois de
  gerar, confira os tamanhos dos três arquivos em `dist\`.

## Modo online

- Toda janela nova aberta pelo motor (`Sessao.EsperarDialogo`, na thread do motor) precisa de
  prazo no jogo online: envolva com `s.ComPrazoSeOnline(() => s.EsperarDialogo(...))`. No
  online o motor roda no host e fica parado até a janela fechar; sem prazo, se o host não
  clicar, o jogo trava para todos. No jogo local ela continua esperando o OK.
  Exemplo: a "Premiação das divisões" em `TelaFimDeTemporada.Mostrar`.
- Detalhes do modo online: `docs/multiplayer-online.md`.
- Testes online (`RetroFoot.Motor.Testes`, filtro `Online`) sem o save do original: copie um
  save criado pelo jogo para `RetroFoot.Motor.Testes/Oraculo/j3-entrada.e98` (ver o
  `LEIA.md` dessa pasta). A conexão real com o Firebase: `dotnet run --project ferramentas/RegrasFirebase`.

## Soaks (ferramentas/SoakUI e SoakOnline)

- Rode no máximo ~6 soaks em paralelo: com mais, a memória do computador do vinibretz se esgota
  (o resto do sistema já ocupa ~28 GB) e os processos morrem com "Out of memory" logo ao abrir,
  o que não é bug do jogo.
- Sem o jogo original, aponte `RETROFOOT_JOGO` para `RetroFoot.App/Assets/Dados`. Os temas são
  escolhidos com `RETROFOOT_VISUAL=moderno` e `RETROFOOT_TEMA=verde|escuro|claro`.
