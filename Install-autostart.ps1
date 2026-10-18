param([Alias('Avinstallera')][switch]$Uninstall)
$ErrorActionPreference='Stop'
$runKey=[Microsoft.Win32.Registry]::CurrentUser.CreateSubKey('Software\Microsoft\Windows\CurrentVersion\Run')
try {
    if($Uninstall) {
        $runKey.DeleteValue('Tumacro',$false)
        $runKey.DeleteValue('TumMakro',$false)
        Write-Output 'Startup entries removed. Application files and settings are preserved.'
        return
    }
    $sourceExe=Join-Path $PSScriptRoot 'Tumacro.exe'
    if(-not (Test-Path -LiteralPath $sourceExe -PathType Leaf)){throw 'Place this script beside Tumacro.exe, or build the app first.'}
    $installDirectory=Join-Path $env:LOCALAPPDATA 'Tumacro'
    $installedExe=Join-Path $installDirectory 'Tumacro.exe'
    $legacyExe=Join-Path $env:LOCALAPPDATA 'TumMakro\TumMakro.exe'
    $desktopDirectory=[Environment]::GetFolderPath('Desktop')
    $knownPaths=@($installedExe,$legacyExe,(Join-Path $desktopDirectory 'TumMakro.exe'),(Join-Path $desktopDirectory 'Tumacro.exe'))
    $runningApps=@(Get-Process -Name Tumacro,TumMakro -ErrorAction SilentlyContinue | Where-Object {$knownPaths -contains $_.Path})
    foreach($runningApp in $runningApps){Stop-Process -Id $runningApp.Id; $runningApp.WaitForExit(3000) | Out-Null}
    New-Item -ItemType Directory -Path $installDirectory -Force | Out-Null
    if([IO.Path]::GetFullPath($sourceExe) -ne [IO.Path]::GetFullPath($installedExe)) {
        for($attempt=0;$attempt -lt 10;$attempt++) {
            try {Copy-Item -LiteralPath $sourceExe -Destination $installedExe -Force; break}
            catch {if($attempt -eq 9){throw}; Start-Sleep -Milliseconds 150}
        }
    }
    $command='"'+$installedExe+'" --background'
    $runKey.SetValue('Tumacro',$command,[Microsoft.Win32.RegistryValueKind]::String)
    if($runKey.GetValue('Tumacro') -ne $command){throw 'Could not verify the startup entry.'}
    $runKey.DeleteValue('TumMakro',$false)
    Start-Process -FilePath $installedExe -ArgumentList '--background' -WindowStyle Hidden
    Write-Output 'Tumacro will start in the system tray at Windows sign-in.'
} finally {$runKey.Dispose()}
