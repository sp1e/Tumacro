param([switch]$Avinstallera)
$ErrorActionPreference = 'Stop'
$runKey = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey('Software\Microsoft\Windows\CurrentVersion\Run')
try {
    if ($Avinstallera) {
        $runKey.DeleteValue('TumMakro', $false)
        Write-Output 'Autostarten är borttagen. TumMakro-filerna finns kvar.'
        return
    }
    $sourceExe = Join-Path $PSScriptRoot 'TumMakro.exe'
    if (-not (Test-Path -LiteralPath $sourceExe -PathType Leaf)) { throw 'Lägg skriptet i samma mapp som TumMakro.exe, eller bygg programmet först.' }
    $installFolder = Join-Path $env:LOCALAPPDATA 'TumMakro'
    $installedExe = Join-Path $installFolder 'TumMakro.exe'
    New-Item -ItemType Directory -Force -Path $installFolder | Out-Null
    if ([IO.Path]::GetFullPath($sourceExe) -ne [IO.Path]::GetFullPath($installedExe)) {
        $runningApps = @(Get-Process -Name TumMakro -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $installedExe })
        foreach ($runningApp in $runningApps) { Stop-Process -Id $runningApp.Id; $runningApp.WaitForExit(3000) | Out-Null }
        for ($attempt=0; $attempt -lt 10; $attempt++) {
            try { Copy-Item -LiteralPath $sourceExe -Destination $installedExe -Force; break }
            catch { if ($attempt -eq 9) { throw }; Start-Sleep -Milliseconds 150 }
        }
    }
    $command = '"' + $installedExe + '" --background'
    $runKey.SetValue('TumMakro', $command, [Microsoft.Win32.RegistryValueKind]::String)
    if ($runKey.GetValue('TumMakro') -ne $command) { throw 'Autostarten kunde inte verifieras.' }
    Start-Process -FilePath $installedExe -ArgumentList '--background' -WindowStyle Hidden
    Write-Output 'TumMakro startar nu i bakgrunden vid Windows-inloggning.'
} finally { $runKey.Dispose() }
