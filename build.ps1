param(
    [switch]$SkipTests,
    [switch]$SkipSpeechTest,
    [switch]$SkipSmoke,
    [switch]$SkipPackage,
    [switch]$TestInstaller
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root 'src'
$build = Join-Path $root 'build'
$dist = Join-Path $root 'dist'
$assets = Join-Path $root 'assets'
$icon = Join-Path $assets 'BossTodo.ico'
$manifest = Join-Path $src 'app.manifest'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path $csc)) { throw "找不到 .NET Framework 4.x 编译器：$csc" }
New-Item -ItemType Directory -Force -Path $build, $dist, $assets, (Join-Path $root 'preview') | Out-Null

$speech = Join-Path $env:WINDIR 'Microsoft.NET\assembly\GAC_MSIL\System.Speech\v4.0_4.0.0.0__31bf3856ad364e35\System.Speech.dll'
$refs = @(
    '/r:System.dll',
    '/r:System.Core.dll',
    '/r:System.Drawing.dll',
    '/r:System.Windows.Forms.dll',
    '/r:System.Web.Extensions.dll',
    ('/r:"' + $speech + '"')
)

function Invoke-Csc([string[]]$arguments) {
    & $csc @arguments
    if ($LASTEXITCODE -ne 0) { throw "csc 编译失败（退出码 $LASTEXITCODE）" }
}

$testExe = Join-Path $build 'BossTodo.Test.exe'
$common = @('/nologo', '/langversion:5', '/warn:4', ('/recurse:"' + $src + '\*.cs"')) + $refs

Write-Host '[1/6] 编译测试用控制台程序...'
Invoke-Csc (@('/target:exe', ('/out:"' + $testExe + '"')) + $common)

if (-not (Test-Path $icon)) {
    Write-Host '      生成多尺寸应用图标...'
    & $testExe --make-icon $icon | Out-Host
    if ($LASTEXITCODE -ne 0) { throw '生成图标失败' }
}

if (-not $SkipTests) {
    Write-Host '[2/6] 运行核心逻辑自测...'
    & $testExe --selftest | Out-Host
    if ($LASTEXITCODE -ne 0) { throw '核心逻辑自测失败' }

    if (-not $SkipSpeechTest) {
        Write-Host '      验证离线语音合成（WAV）...'
        & $testExe --speak-test (Join-Path $build 'speech-test.wav') | Out-Host
        if ($LASTEXITCODE -ne 0) { Write-Warning '语音合成验证失败：请确认 Windows 已安装语音引擎（不影响程序其他功能）' }
    }

    if (-not $SkipSmoke) {
        Write-Host '      运行真实窗口冒烟测试...'
        & $testExe --smoke | Out-Host
        if ($LASTEXITCODE -ne 0) { throw '窗口冒烟测试失败' }
    }

    Write-Host '      渲染界面预览图...'
    & $testExe --render-preview (Join-Path $root 'preview') | Out-Host
    if ($LASTEXITCODE -ne 0) { throw '渲染预览失败' }
}

Write-Host '[3/6] 编译正式版 BossTodo.exe...'
$appExe = Join-Path $build 'BossTodo.exe'
Invoke-Csc (@('/target:winexe', '/optimize+', ('/out:"' + $appExe + '"'),
    ('/win32icon:"' + $icon + '"'), ('/win32manifest:"' + $manifest + '"')) + $common)

Write-Host '[4/6] 编译安装程序 BossTodoSetup.exe...'
$setupExe = Join-Path $build 'BossTodoSetup.exe'
& $csc /nologo /target:winexe /langversion:5 /warn:4 /optimize+ `
    ('/out:"' + $setupExe + '"') ('/win32icon:"' + $icon + '"') `
    ('/win32manifest:"' + (Join-Path $root 'installer\Setup.manifest') + '"') `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    ('/recurse:"' + (Join-Path $root 'installer\*.cs') + '"')
if ($LASTEXITCODE -ne 0) { throw '安装程序编译失败' }

if (-not $SkipPackage) {
    Write-Host '[5/6] 生成便携版与安装包...'
    $readme = Join-Path $root 'README.txt'
    $portableDir = Join-Path $dist 'BossTodo-portable'
    $setupDir = Join-Path $dist 'BossTodo-setup'
    foreach ($dir in @($portableDir, $setupDir)) {
        if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }

    Copy-Item $appExe, $icon -Destination $portableDir
    if (Test-Path $readme) { Copy-Item $readme -Destination $portableDir }
    New-Item -ItemType File -Force -Path (Join-Path $portableDir 'BossTodo.portable') | Out-Null

    Copy-Item $appExe, $icon, $setupExe -Destination $setupDir
    if (Test-Path $readme) { Copy-Item $readme -Destination $setupDir }

    $portableZip = Join-Path $dist 'BossTodo-1.0.0-portable.zip'
    $setupZip = Join-Path $dist 'BossTodo-1.0.0-setup.zip'
    if (Test-Path $portableZip) { Remove-Item $portableZip -Force }
    if (Test-Path $setupZip) { Remove-Item $setupZip -Force }
    Compress-Archive -Path (Join-Path $portableDir '*') -DestinationPath $portableZip
    Compress-Archive -Path (Join-Path $setupDir '*') -DestinationPath $setupZip

    if ($TestInstaller) {
        Write-Host '[6/6] 静默安装 → 校验 → 卸载...'
        $testDir = Join-Path $env:TEMP 'Boss Todo Install Test'
        $desktopLnk = Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'Boss 待办.lnk'
        $startGroup = Join-Path ([Environment]::GetFolderPath('Programs')) 'Boss 待办'
        $uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\BossTodo'
        $uninstallBackup = Join-Path $env:TEMP 'BossTodo-uninstall-entry.reg'
        $hadUninstallEntry = Test-Path $uninstallKey
        if ($hadUninstallEntry) {
            if (Test-Path $uninstallBackup) { Remove-Item $uninstallBackup -Force }
            & reg.exe export 'HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\BossTodo' $uninstallBackup /y | Out-Null
        }
        if (Test-Path $testDir) { Remove-Item $testDir -Recurse -Force -ErrorAction SilentlyContinue }
        if (Test-Path $desktopLnk) { Remove-Item $desktopLnk -Force -ErrorAction SilentlyContinue }
        if (Test-Path $startGroup) { Remove-Item $startGroup -Recurse -Force -ErrorAction SilentlyContinue }

        $install = Start-Process -FilePath $setupExe -ArgumentList @('/S', ('/DIR="' + $testDir + '"')) -Wait -PassThru
        if ($install.ExitCode -ne 0) { throw "静默安装失败（退出码 $($install.ExitCode)）" }
        if (-not (Test-Path (Join-Path $testDir 'BossTodo.exe'))) { throw '安装后未找到 BossTodo.exe' }
        if (-not (Test-Path (Join-Path $testDir 'Uninstall.exe'))) { throw '安装后未找到 Uninstall.exe' }
        if (-not (Test-Path $desktopLnk)) { throw '安装后未创建桌面快捷方式' }
        if (-not (Test-Path (Join-Path $startGroup 'Boss 待办.lnk'))) { throw '安装后未创建开始菜单快捷方式' }
        if (-not (Test-Path (Join-Path $startGroup '卸载 Boss 待办.lnk'))) { throw '安装后未创建卸载快捷方式' }
        if (-not (Test-Path $uninstallKey)) { throw '安装后未写入“应用和功能”卸载项' }
        $entry = Get-ItemProperty $uninstallKey
        if ($entry.UninstallString -notmatch 'Uninstall\.exe') { throw "卸载项 UninstallString 不正确：$($entry.UninstallString)" }
        if ($entry.InstallLocation -ne $testDir) { throw "卸载项 InstallLocation 不正确：$($entry.InstallLocation)" }
        if (-not $entry.QuietUninstallString) { throw '卸载项缺少 QuietUninstallString' }

        # 不带 --uninstall，直接以 /S 启动 Uninstall.exe，验证按文件名自动识别卸载模式
        $uninstall = Start-Process -FilePath (Join-Path $testDir 'Uninstall.exe') -ArgumentList @('/S', ('/DIR="' + $testDir + '"')) -Wait -PassThru
        if ($uninstall.ExitCode -ne 0) { throw "静默卸载失败（退出码 $($uninstall.ExitCode)）" }
        $deadline = (Get-Date).AddSeconds(20)
        while ((Test-Path $testDir) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 400 }
        if (Test-Path $testDir) { throw "卸载后安装目录仍存在（Uninstall.exe 可能被当成安装程序）：$testDir" }
        if (Test-Path $desktopLnk) { throw '卸载后桌面快捷方式仍存在' }
        if (Test-Path $startGroup) { throw '卸载后开始菜单目录仍存在' }
        if (Test-Path $uninstallKey) { throw '卸载后“应用和功能”卸载项仍存在' }
        if ($hadUninstallEntry -and (Test-Path $uninstallBackup)) {
            & reg.exe import $uninstallBackup | Out-Null
            Remove-Item $uninstallBackup -Force -ErrorAction SilentlyContinue
        }
        Write-Host '      静默安装 / 卸载校验通过（含“应用和功能”卸载项、Uninstall.exe 无参数识别）'
    }
}

Write-Host ''
Write-Host '构建完成：'
Get-ChildItem $build -File | Where-Object { $_.Name -match 'BossTodo' } | Select-Object Name, Length | Format-Table -AutoSize | Out-Host
if (-not $SkipPackage) { Get-ChildItem $dist | Select-Object Name, Length | Format-Table -AutoSize | Out-Host }
