$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 ('/out:'+(Join-Path $PSScriptRoot 'TumMakro.exe')) ('/win32manifest:'+(Join-Path $PSScriptRoot 'src\Studio.manifest')) /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Xml.dll (Join-Path $PSScriptRoot 'src\TumMakro.cs') (Join-Path $PSScriptRoot 'src\Studio.cs') (Join-Path $PSScriptRoot 'src\StudioTests.cs')
if($LASTEXITCODE -ne 0){throw 'Bygget misslyckades.'}
Write-Output 'TumMakro Studio 1.0 byggd.'
