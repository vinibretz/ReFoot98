# Como lançar uma versão nova (fork vinibretz)

Passo a passo para publicar uma versão do RetroFoot neste fork, com os pacotes de Windows,
Linux e Android, no mesmo padrão das releases do original. Exemplo com a versão **0.2.4**;
troque pelo número da sua.

Numeração: `maior.menor.correção`. Correção (0.2.**4**) para ajustes e consertos; menor
(0.**3**.0) para novidades maiores. A tag é sempre `v<versão>-beta` (ex.: `v0.2.4-beta`).

## 0. Uma vez só (já feito nesta máquina)

- .NET SDK 10, o workload Android (`dotnet workload install android`), o JDK 17 em
  `%LOCALAPPDATA%\Programs\jdk-17*` e o Android SDK em `%LOCALAPPDATA%\Android\Sdk`.
- A chave de assinatura do Android em `%USERPROFILE%\.config\retrofoot`
  (`retrofoot-android.keystore` e `retrofoot-android.senha`, com a senha em duas linhas).
  **Guarde um backup dessa pasta fora do GitHub.** Sem ela, nenhuma versão nova do APK instala
  por cima da anterior; os jogadores teriam de desinstalar e perder os saves.
- O `master` acompanha o seu repositório (`origin` = vinibretz; `upstream` = o original do
  Juan). Confira com `git branch -vv`: tem de aparecer `[origin/master]`.

## 1. Fazer e testar as mudanças

1. Implemente e teste no jogo (`dotnet run --project RetroFoot.Desktop`).
2. **Registre cada mudança no `CHANGELOG.md`**, na seção `[Não lançado]` (Novidades,
   Correções ou Interno). Vale para todas as sessões: as mudanças se acumulam ali até a
   próxima versão.
3. Rode os testes: `dotnet test RetroFoot.Motor.Testes`. (Cerca de 78 testes falham sempre,
   porque precisam dos arquivos do jogo original; o que importa é não aparecerem falhas novas.
   Os testes online passam com um save seu em `RetroFoot.Motor.Testes/Oraculo/j3-entrada.e98`.)
4. Faça o commit das mudanças (pela extensão do git ou `git commit`).

## 1.5. Fechar o CHANGELOG da versão

Antes de gerar os pacotes, no `CHANGELOG.md`:

1. Releia a seção `[Não lançado]` e confira com `git log v<última>-beta..HEAD --oneline` se
   nada ficou de fora.
2. Troque o título `## [Não lançado]` por `## [0.2.4] - AAAA-MM-DD` (a data do lançamento).
3. Acima dela, crie de novo uma seção vazia `## [Não lançado]` (com Novidades, Correções e
   Interno), para as próximas mudanças.

Essa seção da versão é usada no commit, na tag e na descrição da release (passos 4 e 6).

## 2. Gerar os pacotes

No terminal **PowerShell 7** (`pwsh`), na pasta do projeto:

```powershell
pwsh -File .\scripts\gerar-release.ps1 -Versao 0.2.4
```

(O Windows PowerShell antigo, `powershell.exe` 5.1, não serve: ele não tem o `TarWriter` e o
`.tar.gz` do Linux sai vazio, sem gerar o APK.)

O script:

- muda a versão em `RetroFoot.App/RetroFoot.App.csproj` e `RetroFoot.Android/RetroFoot.Android.csproj`
  (no Android também o código da versão, que só pode crescer: 0.2.4 = 204);
- gera em `dist\` os três arquivos:
  - `refoot98-0.2.4-beta-windows-x64.zip`
  - `refoot98-0.2.4-beta-linux-x64.tar.gz`
  - `refoot98-0.2.4-beta-android.apk` (assinado e conferido)

O Android demora (10 a 20 minutos) e roda em prioridade baixa para não travar o computador.
Para gerar só Windows e Linux: acrescente `-SemAndroid`.

Para corrigir só o Android de uma release já lançada, sem versão nova: com os csproj ainda na
versão lançada, rode com `-SoAndroid` e a mesma versão (ex.: `-Versao 0.2.8 -SoAndroid`), e
troque o `.apk` na release do GitHub (apague o antigo e envie o novo com o mesmo nome). Quem já
instalou o APK antigo não recebe aviso de atualização (a versão é a mesma): precisa baixar e
instalar por cima. Registre a correção na seção da própria versão no `CHANGELOG.md`.

O zip do Windows traz a pasta `ReFoot98` com o `RetroFoot.exe` e o
`LEIA-ME - EXTRAIA ANTES DE JOGAR.txt` (o texto está no script).

Teste o pacote do Windows: extraia o zip numa pasta qualquer, fora do projeto, e abra o
`RetroFoot.exe`. O APK, se puder, instale num celular.

## 3. Atualizar o README

No `README.md`, na tabela **Download**, troque a versão nos três links (`v0.2.3` →
`v0.2.4`, nos dois lugares de cada link). Se a versão tiver novidade, acrescente na lista
"Sobre este fork".

## 4. Commit da versão e tag

A mensagem do commit e a da tag levam as Novidades e Correções da seção da versão no
`CHANGELOG.md` (sem o Interno). Copie os itens para um arquivo temporário, por exemplo
`dist\notas-0.2.4.txt`:

```text
ReFoot98 0.2.4 beta

Novidades:
- ...

Correções:
- ...
```

```powershell
git add CHANGELOG.md README.md RetroFoot.App/RetroFoot.App.csproj RetroFoot.Android/RetroFoot.Android.csproj
git commit -F dist\notas-0.2.4.txt
git tag -a v0.2.4-beta -F dist\notas-0.2.4.txt
```

(`dist\` não vai para o git, então o arquivo de notas não entra no commit.)

## 5. Enviar para o GitHub

```powershell
git push origin master --tags
```

(Ou pela extensão: "Sincronizar Alterações" e depois, no menu `...` do Controle do Código-Fonte,
**Push Tags** / "Enviar Marcas".)

**Não use** o painel "Solicitação de Pull" (Pull Request): ele propõe as suas mudanças ao
repositório do Juan.

## 6. Criar a release no GitHub

1. Abra https://github.com/vinibretz/ReFoot98/releases/new
2. **Choose a tag**: `v0.2.4-beta`.
3. **Release title**: `ReFoot98 0.2.4 beta` (o nome do jogo é ReFoot98, não RetroFoot).
4. **Descrição**: use o modelo abaixo; em **Novidades** e **Correções**, copie os itens da
   seção da versão no `CHANGELOG.md` (o Interno fica de fora). Mantenha os títulos
   `### Novidades` e `### Correções` e os itens em `- `: o aviso de atualização do jogo lê
   essas duas seções da descrição e mostra os itens ao jogador (`Atualizacao.Notas`).
5. Arraste os três arquivos de `dist\` para "Attach binaries" e espere terminar de enviar
   (o APK tem uns 140 MB).
6. Marque **Set as a pre-release**.
7. **Publish release**.

Assim que a release sai, quem já tem o jogo (0.2.3 ou mais nova) recebe o aviso de
atualização ao abrir, no Windows, no Linux e no Android.

Modelo de descrição:

```markdown
## ReFoot98 0.2.4 beta

> **Versão beta:** espere bugs. Encontrou um problema? Abra uma [issue](https://github.com/vinibretz/ReFoot98/issues).

Versão modificada do [RetroFoot de Juan Silveira](https://github.com/juan-silveira/RetroFoot), sob a mesma licença (GPL v3).

### Downloads

| Sistema | Arquivo |
|---|---|
| 🐧 Linux (x64) | `refoot98-0.2.4-beta-linux-x64.tar.gz` |
| 🪟 Windows (x64) | `refoot98-0.2.4-beta-windows-x64.zip` |
| 🤖 Android | `refoot98-0.2.4-beta-android.apk` |

> [!IMPORTANT]
> **Windows: extraia o zip antes de jogar** (botão direito → **Extrair tudo...**) e abra o
> `RetroFoot.exe` da pasta extraída. Aberto de dentro do zip, o jogo roda numa pasta
> temporária e os saves se perdem.

### Novidades

- ...

### Correções

- ...
```

## Trazer novidades do original (Juan)

Quando o Juan lançar versões novas e você quiser trazer as mudanças dele:

```powershell
git fetch upstream
git merge upstream/master
```

Resolva os conflitos, se houver. Os mais comuns:

- `RetroFoot.App/Atualizacao.cs`: mantenha `repos/vinibretz/ReFoot98`.
- `RetroFoot.Android/RetroFoot.Android.csproj`: mantenha `io.github.vinibretz.retrofoot` e a
  `PastaChaves`.
- `README.md`: mantenha os links do fork.

A sua próxima versão tem de ter um número **maior** que o da sua última (e, de preferência,
que o da última do Juan, para não confundir). Depois, siga do passo 1.

## Problemas conhecidos

- **"publish win-x64 falhou" ou build do Android com "Out of memory" / "Could not process the
  body of method"** (ILLink): o servidor de build e os nós do MSBuild que sobram de builds
  anteriores seguram memória e arquivos, e o ILLink falha mesmo com memória livre. O script já
  trata isso: roda `dotnet build-server shutdown` antes de cada publish e, se falhar, tenta de
  novo uma vez (aparece "falhou; ... tentando de novo" em amarelo, e está tudo certo). Se
  falhar duas vezes, o log fica em `dist\publish-<sistema>-erro.log` ou
  `dist\android-erro.log`; feche programas pesados e rode o script de novo.
- O Android já gera uma ABI de cada vez (as três em paralelo esgotam a memória).
- **"Chave do Android não encontrada"**: restaure o backup de `%USERPROFILE%\.config\retrofoot`.
- **Avisos (warnings) amarelos no build**: são do código original e não impedem nada.
- **A release não aparece como atualização no jogo**: confira se a versão é maior que a
  instalada e se os nomes dos arquivos terminam em `windows-x64.zip`, `linux-x64.tar.gz` e
  `android.apk`.
