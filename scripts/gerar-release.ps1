# Gera os pacotes de uma release do ReFoot98 (fork vinibretz do RetroFoot) no Windows:
#   dist/refoot98-<versao>-beta-windows-x64.zip   (RetroFoot.exe: o nome que a atualizacao procura)
#   dist/refoot98-<versao>-beta-linux-x64.tar.gz  (RetroFoot)
#   dist/refoot98-<versao>-beta-android.apk       (assinado com a chave do fork)
# Antes, muda a versao nos dois csproj. Nao faz commit, tag nem push (passo a passo em
# docs/release.md).
#
# Uso (PowerShell, na raiz do repositorio):
#   .\scripts\gerar-release.ps1 -Versao 0.2.4
#   .\scripts\gerar-release.ps1 -Versao 0.2.4 -SemAndroid
param(
    [Parameter(Mandatory)][ValidatePattern('^\d+\.\d+\.\d+$')][string]$Versao,
    [switch]$SemAndroid
)
$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
Set-Location $raiz
$nome = "refoot98-$Versao-beta"
$dist = Join-Path $raiz 'dist'

# 1. a versao: o jogo (Sobre e atualizacao) e o Android (o codigo so cresce: 0.2.4 = 204)
$p = $Versao.Split('.') | ForEach-Object { [int]$_ }
$codigo = $p[0] * 10000 + $p[1] * 100 + $p[2]
$app = 'RetroFoot.App\RetroFoot.App.csproj'
$and = 'RetroFoot.Android\RetroFoot.Android.csproj'
(Get-Content $app -Raw) -replace '<Version>[^<]*</Version>', "<Version>$Versao</Version>" | Set-Content $app -NoNewline
(Get-Content $and -Raw) `
    -replace '<ApplicationDisplayVersion>[^<]*<', "<ApplicationDisplayVersion>$Versao<" `
    -replace '<ApplicationVersion>[^<]*<', "<ApplicationVersion>$codigo<" `
    -replace '\(\d+\.\d+\.\d+ = \d+\)', "($Versao = $codigo)" | Set-Content $and -NoNewline
Write-Host "Versao $Versao (Android $codigo)" -ForegroundColor Cyan

New-Item -ItemType Directory -Force $dist | Out-Null
$tmp = Join-Path $dist 'tmp'
if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force }

# 2. Windows e Linux: um executavel so, renomeado para RetroFoot(.exe) (o nome que a
#    atualizacao automatica procura dentro do pacote)
foreach ($r in 'win-x64', 'linux-x64') {
    Write-Host "Publicando $r..." -ForegroundColor Cyan
    dotnet publish RetroFoot.Desktop -c Release -r $r -o "$tmp\$r" | Out-Null
    if ($LASTEXITCODE) { throw "publish $r falhou (rode o comando a mao para ver o erro)" }
}
Move-Item "$tmp\win-x64\RetroFoot.Desktop.exe" "$tmp\RetroFoot.exe"
Compress-Archive "$tmp\RetroFoot.exe" "$dist\$nome-windows-x64.zip" -Force
Move-Item "$tmp\linux-x64\RetroFoot.Desktop" "$tmp\RetroFoot"
# o tar do .NET, com a permissao de execucao (0755) que o Linux precisa, comprimido em gzip
$tgz = [IO.File]::Create("$dist\$nome-linux-x64.tar.gz")
$gz = [IO.Compression.GZipStream]::new($tgz, [IO.Compression.CompressionLevel]::Optimal)
$w = [Formats.Tar.TarWriter]::new($gz, [Formats.Tar.TarEntryFormat]::Gnu, $false)
$e = [Formats.Tar.GnuTarEntry]::new([Formats.Tar.TarEntryType]::RegularFile, 'RetroFoot')
$e.Mode = [IO.UnixFileMode]'UserRead, UserWrite, UserExecute, GroupRead, GroupExecute, OtherRead, OtherExecute'
$e.ModificationTime = [DateTimeOffset]::Now
$e.DataStream = [IO.File]::OpenRead("$tmp\RetroFoot")
$w.WriteEntry($e)
$e.DataStream.Dispose(); $w.Dispose(); $gz.Dispose(); $tgz.Dispose()

# 3. Android: uma ABI de cada vez (em paralelo o ILLink fica sem memoria e trava a maquina),
#    em prioridade baixa; depois assina sempre a mao e confere (o build pode pular a
#    assinatura e deixar o -Signed.apk sem assinatura)
if (-not $SemAndroid) {
    $jdk = (Get-ChildItem "$env:LOCALAPPDATA\Programs" -Directory -Filter 'jdk-17*' | Select-Object -First 1).FullName
    $sdk = "$env:LOCALAPPDATA\Android\Sdk"
    $chaves = "$env:USERPROFILE\.config\retrofoot"
    if (-not $jdk) { throw 'JDK 17 nao encontrado em %LOCALAPPDATA%\Programs' }
    if (-not (Test-Path "$chaves\retrofoot-android.keystore")) { throw "Chave do Android nao encontrada em $chaves (restaure o backup)" }
    Write-Host 'Publicando Android (demora; uma ABI de cada vez)...' -ForegroundColor Cyan
    $argumentos = "publish RetroFoot.Android -c Release -f net10.0-android -m:1 -p:BuildInParallel=false `"-p:AndroidSdkDirectory=$sdk`" `"-p:JavaSdkDirectory=$jdk`" -o `"$tmp\android`""
    $proc = Start-Process dotnet -ArgumentList $argumentos -NoNewWindow -PassThru -RedirectStandardOutput "$tmp\android.log"
    try { $proc.PriorityClass = 'BelowNormal' } catch {}
    $proc.WaitForExit()
    $apk = Get-ChildItem "$tmp\android" -Filter '*.apk' -ErrorAction SilentlyContinue | Where-Object Name -notlike '*-Signed.apk' | Select-Object -First 1
    if (-not $apk) { throw "build do Android falhou (veja $tmp\android.log)" }
    $signer = (Get-ChildItem "$sdk\build-tools" -Directory | Sort-Object Name -Descending | Select-Object -First 1).FullName + '\lib\apksigner.jar'
    $saida = "$dist\$nome-android.apk"
    & "$jdk\bin\java.exe" -jar $signer sign --ks "$chaves\retrofoot-android.keystore" --ks-key-alias retrofoot `
        --ks-pass "file:$chaves\retrofoot-android.senha" --key-pass "file:$chaves\retrofoot-android.senha" --out $saida $apk.FullName
    if ($LASTEXITCODE) { throw 'assinatura do APK falhou' }
    Remove-Item "$saida.idsig" -ErrorAction SilentlyContinue
    $v = & "$jdk\bin\java.exe" -jar $signer verify -v $saida 2>&1
    if (-not ($v -match '^Verifies')) { throw "APK nao verifica: $v" }
}

Remove-Item $tmp -Recurse -Force
Write-Host "`nPronto. Pacotes em dist\:" -ForegroundColor Green
Get-ChildItem $dist -File -Filter "$nome*" | ForEach-Object { '  {0}  ({1:N1} MB)' -f $_.Name, ($_.Length / 1MB) }
Write-Host "`nProximos passos: docs\release.md (testar, commit, tag v$Versao-beta, push, release no GitHub)."
