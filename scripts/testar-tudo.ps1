# Testa o que da para testar sem jogar (docs/testes.md), antes de lancar uma versao:
#   1. os testes do motor (RetroFoot.Motor.Testes) e os da interface (RetroFoot.App.Testes: as
#      telas em cada visual, as opcoes da versao Atual, as Configuracoes);
#   2. tres soaks em paralelo: uma temporada do jogo com as opcoes da versao Atual ligadas, uma
#      com elas desligadas e 3 jornadas do online (host e convidado: cada jornada espera o prazo
#      de ficar pronto, uns 3 min);
#   3. as capturas das telas em cada visual (Classico e Moderno, 4:3 e 16:9, os temas, o
#      celular), com tudo ligado e tudo desligado, numa pagina para passar o olho.
# Tudo vai para dist\testes\<data>\ (o resumo.txt, os logs, as capturas e o index.html).
#
# Uso (PowerShell 7, na raiz do repositorio):
#   pwsh -File .\scripts\testar-tudo.ps1
#   pwsh -File .\scripts\testar-tudo.ps1 -SemSoaks -SemCapturas   (so os testes)
param(
    [switch]$SemSoaks,
    [switch]$SemCapturas
)
$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
Set-Location $raiz
$saida = Join-Path $raiz ("dist\testes\" + (Get-Date -Format 'yyyy-MM-dd_HH-mm'))
New-Item -ItemType Directory -Force $saida | Out-Null
$dados = Join-Path $raiz 'RetroFoot.App\Assets\Dados'
$resultados = [System.Collections.Generic.List[object]]::new()

function Passo([string]$nome, [scriptblock]$corpo) {
    Write-Host "== $nome" -ForegroundColor Cyan
    $t = [Diagnostics.Stopwatch]::StartNew()
    $detalhe = ''
    try { $r = & $corpo; $ok = [bool]$r.Ok; $detalhe = $r.Detalhe }
    catch { $ok = $false; $detalhe = $_.Exception.Message }
    $t.Stop()
    $cor = if ($ok) { 'Green' } else { 'Red' }
    Write-Host ("   {0} ({1:mm\:ss}) {2}" -f ($(if ($ok) { 'OK' } else { 'FALHOU' })), $t.Elapsed, $detalhe) -ForegroundColor $cor
    $resultados.Add([pscustomobject]@{ Passo = $nome; Ok = $ok; Tempo = $t.Elapsed; Detalhe = $detalhe })
}

# a linha do total do dotnet test ("Aprovado! - Com falha: 0, Aprovado: 235, Ignorado: 23...")
function Teste([string]$projeto, [string]$log) {
    dotnet test $projeto *> $log
    $total = Select-String -Path $log -Pattern '(Aprovado|Com falha|Passed|Failed)!.*Total' | Select-Object -Last 1
    @{ Ok = ($LASTEXITCODE -eq 0); Detalhe = if ($total) { ($total.Line -replace '^.*?(Com falha|Failed)', '$1').Trim() } else { "ver $log" } }
}

# ------------------------------------------------------------------ 1. os testes
Passo 'Testes do motor' { Teste 'RetroFoot.Motor.Testes' (Join-Path $saida 'testes-motor.log') }
Passo 'Testes da interface' { Teste 'RetroFoot.App.Testes' (Join-Path $saida 'testes-interface.log') }

# ------------------------------------------------------------------ 2. os soaks
$save = Join-Path $saida 'soak-ligado.e98'
if (-not $SemSoaks) {
    Passo 'Soaks (ligado, desligado e online)' {
        dotnet build ferramentas\SoakUI *> (Join-Path $saida 'build-soakui.log')
        if ($LASTEXITCODE -ne 0) { return @{ Ok = $false; Detalhe = 'o build do SoakUI falhou' } }
        dotnet build ferramentas\SoakOnline *> (Join-Path $saida 'build-soakonline.log')
        if ($LASTEXITCODE -ne 0) { return @{ Ok = $false; Detalhe = 'o build do SoakOnline falhou' } }
        $soaks = @(
            @{ Nome = 'ligado'; Exe = 'ferramentas\SoakUI\bin\Debug\net8.0\SoakUI.exe'; Args = '8 mod 1 1 1234'; Env = @{ GUARDAR_SAVE = $save } },
            @{ Nome = 'desligado'; Exe = 'ferramentas\SoakUI\bin\Debug\net8.0\SoakUI.exe'; Args = '8 mod 1 1 4321'; Env = @{ SEM_AMARELOS = '1'; SEM_NOTA = '1'; SEM_INGRESSO = '1'; SEM_CAMPINHO = '1' } },
            @{ Nome = 'online'; Exe = 'ferramentas\SoakOnline\bin\Debug\net8.0\SoakOnline.exe'; Args = '8 mod 1 1234'; Env = @{ SOAK_JORNADAS = '3' } }
        )
        $procs = foreach ($s in $soaks) {
            $env:RETROFOOT_JOGO = $dados
            foreach ($k in $s.Env.Keys) { Set-Item "env:$k" $s.Env[$k] }
            $p = Start-Process -FilePath (Join-Path $raiz $s.Exe) -ArgumentList $s.Args -PassThru -NoNewWindow `
                -RedirectStandardOutput (Join-Path $saida "soak-$($s.Nome).log") -RedirectStandardError (Join-Path $saida "soak-$($s.Nome)-erro.log")
            foreach ($k in $s.Env.Keys) { Remove-Item "env:$k" -ErrorAction SilentlyContinue }
            [pscustomobject]@{ Nome = $s.Nome; P = $p }
        }
        $falhas = @()
        foreach ($x in $procs) {
            if (-not $x.P.WaitForExit(30 * 60 * 1000)) { $x.P.Kill(); $falhas += "$($x.Nome): passou de 30 min" ; continue }
            if ($x.P.ExitCode -ne 0) { $falhas += "$($x.Nome): codigo $($x.P.ExitCode) (soak-$($x.Nome).log)" }
        }
        @{ Ok = ($falhas.Count -eq 0); Detalhe = if ($falhas) { $falhas -join '; ' } else { 'os 3 terminaram sem erro' } }
    }
}

# ------------------------------------------------------------------ 3. as capturas
if (-not $SemCapturas) {
    Passo 'Capturas das telas' {
        dotnet build ferramentas\FichasPrint *> (Join-Path $saida 'build-fichasprint.log')
        if ($LASTEXITCODE -ne 0) { return @{ Ok = $false; Detalhe = 'o build do FichasPrint falhou' } }
        # o save do soak (ou um local, se o soak nao rodou)
        $entrada = if (Test-Path $save) { $save } else {
            @('RetroFoot.Motor.Testes\Oraculo\j3-entrada.e98') + @(Get-ChildItem 'RetroFoot.Motor.Testes\SavesLocais\*.e98' -ErrorAction SilentlyContinue | ForEach-Object FullName) |
                Where-Object { Test-Path $_ } | Select-Object -First 1
        }
        if (-not $entrada) { return @{ Ok = $false; Detalhe = 'sem save para as capturas (rode sem -SemSoaks)' } }
        $exe = Join-Path $raiz 'ferramentas\FichasPrint\bin\Debug\net8.0\FichasPrint.exe'
        $visuais = [ordered]@{
            'Clássico 4:3' = @{ RETROFOOT_VISUAL = 'classico'; RETROFOOT_FORMATO = '43' }
            'Clássico 16:9' = @{ RETROFOOT_VISUAL = 'classico'; RETROFOOT_FORMATO = '169' }
            'Moderno 4:3 verde' = @{ RETROFOOT_VISUAL = 'moderno'; RETROFOOT_FORMATO = '43'; RETROFOOT_TEMA = 'verde' }
            'Moderno 16:9 verde' = @{ RETROFOOT_VISUAL = 'moderno'; RETROFOOT_FORMATO = '169'; RETROFOOT_TEMA = 'verde' }
            'Moderno 16:9 escuro' = @{ RETROFOOT_VISUAL = 'moderno'; RETROFOOT_FORMATO = '169'; RETROFOOT_TEMA = 'escuro' }
            'Moderno 4:3 claro' = @{ RETROFOOT_VISUAL = 'moderno'; RETROFOOT_FORMATO = '43'; RETROFOOT_TEMA = 'claro' }
            'Celular Moderno' = @{ RETROFOOT_VISUAL = 'moderno'; RETROFOOT_CELULAR = '1'; RETROFOOT_TEMA = 'verde' }
            'Celular Retrô' = @{ RETROFOOT_VISUAL = 'classico'; RETROFOOT_CELULAR = '1' }
        }
        $telas = [ordered]@{
            'Configurações' = @('config', $null); 'Equipa: jogador' = @('equipa', 'jogador'); 'Equipa: finanças' = @('equipa', 'financas')
            'Equipa: árbitro' = @('equipa', 'arbitro'); 'Procurar' = @('procura', $null); 'Ficha (Elenco)' = @('plantel', $null); 'Leilão' = @('leilao', $null)
        }
        $combos = [ordered]@{ 'tudo ligado' = '1'; 'tudo desligado' = '0' }
        $pasta = Join-Path $saida 'capturas'
        $trabalhos = foreach ($t in $telas.Keys) { foreach ($v in $visuais.Keys) { foreach ($c in $combos.Keys) {
            [pscustomobject]@{ Tela = $t; Modo = $telas[$t][0]; Pagina = $telas[$t][1]; Visual = $v; Env = $visuais[$v]; Combo = $c; Bit = $combos[$c]
                Pasta = Join-Path $pasta ("{0}-{1}-{2}" -f $telas.Keys.IndexOf($t), [array]::IndexOf(@($visuais.Keys), $v), $combos[$c]) }
        } } }
        # (no maximo 6 ao mesmo tempo: a memoria do computador)
        $feitos = $trabalhos | ForEach-Object -ThrottleLimit 6 -Parallel {
            $w = $_
            New-Item -ItemType Directory -Force $w.Pasta | Out-Null
            $psi = [Diagnostics.ProcessStartInfo]::new($using:exe)
            foreach ($a in @($using:entrada, $w.Pasta, $w.Modo)) { $psi.ArgumentList.Add($a) }
            $psi.UseShellExecute = $false; $psi.RedirectStandardOutput = $true; $psi.RedirectStandardError = $true
            $psi.Environment['RETROFOOT_JOGO'] = $using:dados
            foreach ($k in $w.Env.Keys) { $psi.Environment[$k] = $w.Env[$k] }
            foreach ($k in 'RETROFOOT_AMARELOS', 'RETROFOOT_NOTA', 'RETROFOOT_INGRESSO', 'RETROFOOT_CAMPINHO') { $psi.Environment[$k] = $w.Bit }
            if ($w.Pagina) { $psi.Environment['RETROFOOT_PAGINA'] = $w.Pagina }
            $p = [Diagnostics.Process]::Start($psi)
            $out = $p.StandardOutput.ReadToEnd() + $p.StandardError.ReadToEnd()
            $p.WaitForExit()
            $png = Get-ChildItem $w.Pasta -Filter *.png -ErrorAction SilentlyContinue | Select-Object -First 1
            $w | Add-Member -PassThru Png ($png ? $png.FullName : $null) | Add-Member -PassThru Erro ($png ? '' : $out)
        }
        # a pagina: uma secao por tela, uma linha por visual, ligado e desligado lado a lado
        $h = [System.Text.StringBuilder]::new()
        [void]$h.Append('<!DOCTYPE html><html lang="pt-BR"><meta charset="utf-8"><title>Capturas dos testes</title><style>body{font-family:sans-serif;background:#222;color:#eee;margin:16px}h2{margin-top:32px}table{border-collapse:collapse}td{padding:4px;vertical-align:top}img{max-width:620px;border:1px solid #555}.erro{color:#f88;white-space:pre-wrap;max-width:620px}</style><h1>Capturas dos testes</h1>')
        [void]$h.Append("<p>$(Get-Date -Format 'dd/MM/yyyy HH:mm') &middot; save: $([IO.Path]::GetFileName($entrada)). Clique numa imagem para ver no tamanho real.</p>")
        foreach ($t in $telas.Keys) {
            [void]$h.Append("<h2>$t</h2><table><tr><td></td>" + (($combos.Keys | ForEach-Object { "<td><b>$_</b></td>" }) -join '') + '</tr>')
            foreach ($v in $visuais.Keys) {
                [void]$h.Append("<tr><td><b>$v</b></td>")
                foreach ($c in $combos.Keys) {
                    $f = $feitos | Where-Object { $_.Tela -eq $t -and $_.Visual -eq $v -and $_.Combo -eq $c } | Select-Object -First 1
                    if ($f.Png) {
                        $rel = [IO.Path]::GetRelativePath($saida, $f.Png).Replace('\', '/')
                        [void]$h.Append("<td><a href=""$rel""><img src=""$rel"" loading=""lazy""></a></td>")
                    } else { [void]$h.Append("<td class=""erro"">falhou: $([Net.WebUtility]::HtmlEncode($f.Erro))</td>") }
                }
                [void]$h.Append('</tr>')
            }
            [void]$h.Append('</table>')
        }
        [void]$h.Append('</html>')
        Set-Content -Path (Join-Path $saida 'index.html') -Value $h.ToString() -Encoding utf8
        $semPng = @($feitos | Where-Object { -not $_.Png }).Count
        @{ Ok = ($semPng -eq 0); Detalhe = "$(@($feitos).Count - $semPng) de $(@($feitos).Count) capturas; abra $(Join-Path $saida 'index.html')" }
    }
}

# ------------------------------------------------------------------ o resumo
$linhas = $resultados | ForEach-Object { "{0,-8} {1,-52} {2:mm\:ss}  {3}" -f ($(if ($_.Ok) { 'OK' } else { 'FALHOU' })), $_.Passo, $_.Tempo, $_.Detalhe }
Set-Content -Path (Join-Path $saida 'resumo.txt') -Value $linhas -Encoding utf8
Write-Host ''
Write-Host 'Resumo:' -ForegroundColor Cyan
$linhas | ForEach-Object { Write-Host "  $_" }
Write-Host "  (tudo em $saida)"
if ($resultados | Where-Object { -not $_.Ok }) { exit 1 }
