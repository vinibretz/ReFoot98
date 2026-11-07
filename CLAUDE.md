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

## Testes

- Antes de lançar, `pwsh -File .\scripts\testar-tudo.ps1` (detalhes em `docs/testes.md`): os testes
  do motor e da interface, 3 soaks e as capturas de cada tela em cada visual (`dist\testes\<data>\index.html`).
- Mudou uma tela que as opções da versão Atual afetam (ou criou uma)? Rode `RetroFoot.App.Testes`:
  os testes abrem as telas nos 8 visuais e acusam texto sobreposto ou cortado (nos 4 idiomas) e
  opção que não esconde o que é dela. Tela nova entra em `Cena` e em `LayoutTestes.Telas`; para
  ver o que a tela desenha, o `Despejo` (`DESPEJO=pasta`, `DESPEJO_TELA=...`).

## Release

- Rode `scripts/gerar-release.ps1` sempre com o PowerShell 7: `pwsh -File .\scripts\gerar-release.ps1 -Versao X.Y.Z`.
  Com o Windows PowerShell antigo (`powershell.exe` 5.1) falta o `TarWriter`: o `.tar.gz` do
  Linux sai com 0 bytes e o APK não é gerado, mas o script termina com código 0. Depois de
  gerar, confira os tamanhos dos três arquivos em `dist\`.
- Falhas de memória do ILLink e arquivos travados no publish: o script já libera o servidor de
  build e tenta de novo uma vez. Não refaça os passos à mão; se falhar duas vezes, leia o
  `dist\*-erro.log` (ver "Problemas conhecidos" no `docs/release.md`).
- Faça commit e tag da release pelo PowerShell: pelo Bash, o `git add` já deu "Permission
  denied" em `.git/objects`.

## Modo online

- Toda janela nova aberta pelo motor (`Sessao.EsperarDialogo`, na thread do motor) precisa de
  prazo no jogo online: envolva com `s.ComPrazoSeOnline(() => s.EsperarDialogo(...))`. No
  online o motor roda no host e fica parado até a janela fechar; sem prazo, se o host não
  clicar, o jogo trava para todos. No jogo local ela continua esperando o OK.
  Exemplo: a "Premiação das divisões" em `TelaFimDeTemporada.Mostrar`.
- Regras do jogo online: valem as do jogo (gravadas no `regras.json` e publicadas em
  `info/opcoes`), nunca as Configurações de cada aparelho. Ao criar ou mudar uma opção que
  altera o jogo (uma regra nova nas Configurações, em `Regras` ou nas `Opcoes` do motor),
  atualize junto:
  - `Sala.OpcoesJson` e `Sala.ComOpcoesDoHost` (o que vai para a sala e o que o convidado aplica);
  - `RegrasOnline` (o resumo dos convidados; o editor do host, se a opção puder mudar com o jogo
    em andamento, ou a linha "Não muda", se não puder);
  - o teste `OnlineOpcoesDoHostTestes.TodasAsRegrasPelaSala`;
  - a seção "Regras" do `docs/multiplayer-online.md`.
  Preferências só de tela (idioma, visual, o que mostrar) ficam no aparelho e não entram.
  Vale também para as **Opções do jogo** (`OpcoesDoJogo`, o diálogo Opções): uma opção que muda
  o resultado ou para o jogo de todos (os pênaltis da CPU, o intervalo) vira regra da sala; uma
  que só mostra, mas que no motor do host decidiria pelos outros (demissões), fica fixa no motor do
  online (`OpcoesDoJogo.NoJogoOnline`) e cada aparelho mostra conforme a própria escolha
  (`OpcoesDoJogo.Preferencias`, os filtros do `JanelasOnline`). O sorteio da copa não aparece
  no online (a animação parava o jogo de todos).
- Detalhes do modo online: `docs/multiplayer-online.md`.
- Testes online (`RetroFoot.Motor.Testes`, filtro `Online`) sem o save do original: copie um
  save criado pelo jogo para `RetroFoot.Motor.Testes/Oraculo/j3-entrada.e98` (ver o
  `LEIA.md` dessa pasta). A conexão real com o Firebase: `dotnet run --project ferramentas/RegrasFirebase`.
- Testes que leem saves reais: `RetroFoot.Motor.Testes/saves-locais.txt` (fora do git) aponta
  para `RetroFoot.Motor.Testes/SavesLocais` (saves do jogo e do SoakUI com `GUARDAR_SAVE`). Não
  ponha lá save com "Sobem e descem" escolhido (`MarcaNoSave` falha). Ao trocar esses saves,
  registre a impressão nova que `SemAcrescimosIgualAoOriginal` mostra. Sem o jogo original
  (pasta do jogo, oráculo e `j3` verdadeiro), 23 testes aparecem como ignorados: é esperado;
  a suíte deve ficar sem falhas. Teste novo que precisa desses arquivos: use os atributos de
  `RetroFoot.Motor.Testes/Requisitos.cs` (`[FactComOriginal]`, `[FactComArquivo(pasta, arquivo)]`).

## Soaks (ferramentas/SoakUI e SoakOnline)

- Rode no máximo ~6 soaks em paralelo: com mais, a memória do computador do vinibretz se esgota
  (o resto do sistema já ocupa ~28 GB) e os processos morrem com "Out of memory" logo ao abrir,
  o que não é bug do jogo.
- Sem o jogo original, aponte `RETROFOOT_JOGO` para `RetroFoot.App/Assets/Dados`. Os temas são
  escolhidos com `RETROFOOT_VISUAL=moderno` e `RETROFOOT_TEMA=verde|escuro|claro`.
