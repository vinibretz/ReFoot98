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
2. Rode os testes: `dotnet test RetroFoot.Motor.Testes`. (Cerca de 78 testes falham sempre,
   porque precisam dos arquivos do jogo original; o que importa é não aparecerem falhas novas.)
3. Faça o commit das mudanças (pela extensão do git ou `git commit`).

## 2. Gerar os pacotes

No terminal (PowerShell), na pasta do projeto:

```powershell
.\scripts\gerar-release.ps1 -Versao 0.2.4
```

O script:

- muda a versão em `RetroFoot.App/RetroFoot.App.csproj` e `RetroFoot.Android/RetroFoot.Android.csproj`
  (no Android também o código da versão, que só pode crescer: 0.2.4 = 204);
- gera em `dist\` os três arquivos:
  - `retrofoot-0.2.4-beta-windows-x64.zip`
  - `retrofoot-0.2.4-beta-linux-x64.tar.gz`
  - `retrofoot-0.2.4-beta-android.apk` (assinado e conferido)

O Android demora (10 a 20 minutos) e roda em prioridade baixa para não travar o computador.
Para gerar só Windows e Linux: acrescente `-SemAndroid`.

Teste o pacote do Windows: extraia o zip numa pasta qualquer, fora do projeto, e abra o
`RetroFoot.exe`. O APK, se puder, instale num celular.

## 3. Atualizar o README

No `README.md`, na tabela **Download**, troque a versão nos três links (`v0.2.3` →
`v0.2.4`, nos dois lugares de cada link). Se a versão tiver novidade, acrescente na lista
"Sobre este fork".

## 4. Commit da versão e tag

```powershell
git add README.md RetroFoot.App/RetroFoot.App.csproj RetroFoot.Android/RetroFoot.Android.csproj
git commit -m "Versao 0.2.4"
git tag -a v0.2.4-beta -m "RetroFoot 0.2.4 beta"
```

## 5. Enviar para o GitHub

```powershell
git push origin master --tags
```

(Ou pela extensão: "Sincronizar Alterações" e depois, no menu `...` do Controle do Código-Fonte,
**Push Tags** / "Enviar Marcas".)

**Não use** o painel "Solicitação de Pull" (Pull Request): ele propõe as suas mudanças ao
repositório do Juan.

## 6. Criar a release no GitHub

1. Abra https://github.com/vinibretz/RetroFoot/releases/new
2. **Choose a tag**: `v0.2.4-beta`.
3. **Release title**: `RetroFoot 0.2.4 beta`.
4. **Descrição**: use o modelo abaixo, com as novidades da versão.
5. Arraste os três arquivos de `dist\` para "Attach binaries" e espere terminar de enviar
   (o APK tem uns 140 MB).
6. Marque **Set as a pre-release**.
7. **Publish release**.

Assim que a release sai, quem já tem o jogo (0.2.3 ou mais nova) recebe o aviso de
atualização ao abrir, no Windows, no Linux e no Android.

Modelo de descrição:

```markdown
## RetroFoot 0.2.4 beta

> **Versão beta:** espere bugs. Encontrou um problema? Abra uma [issue](https://github.com/vinibretz/RetroFoot/issues).

Versão modificada do [RetroFoot de Juan Silveira](https://github.com/juan-silveira/RetroFoot), sob a mesma licença (GPL v3).

### Downloads

| Sistema | Arquivo |
|---|---|
| 🐧 Linux (x64) | `retrofoot-0.2.4-beta-linux-x64.tar.gz` |
| 🪟 Windows (x64) | `retrofoot-0.2.4-beta-windows-x64.zip` |
| 🤖 Android | `retrofoot-0.2.4-beta-android.apk` |

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

- `RetroFoot.App/Atualizacao.cs`: mantenha `repos/vinibretz/RetroFoot`.
- `RetroFoot.Android/RetroFoot.Android.csproj`: mantenha `io.github.vinibretz.retrofoot` e a
  `PastaChaves`.
- `README.md`: mantenha os links do fork.

A sua próxima versão tem de ter um número **maior** que o da sua última (e, de preferência,
que o da última do Juan, para não confundir). Depois, siga do passo 1.

## Problemas conhecidos

- **"Out of memory" no build do Android**: aparece se gerar as três ABIs em paralelo; o script
  já gera uma de cada vez. Feche programas pesados se ainda acontecer.
- **"Chave do Android não encontrada"**: restaure o backup de `%USERPROFILE%\.config\retrofoot`.
- **Avisos (warnings) amarelos no build**: são do código original e não impedem nada.
- **A release não aparece como atualização no jogo**: confira se a versão é maior que a
  instalada e se os nomes dos arquivos terminam em `windows-x64.zip`, `linux-x64.tar.gz` e
  `android.apk`.
