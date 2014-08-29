Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Building Go To Window..." -ForegroundColor Green

Function GetMSBuildExe {
	[CmdletBinding()]
	$DotNetVersion = "4.0"
	$RegKey = "HKLM:\software\Microsoft\MSBuild\ToolsVersions\$DotNetVersion"
	$RegProperty = "MSBuildToolsPath"
	$MSBuildExe = Join-Path -Path (Get-ItemProperty $RegKey).$RegProperty -ChildPath "msbuild.exe"
	Return $MSBuildExe
}

Function ZipFiles($Filename, $Source)
{
   Add-Type -Assembly System.IO.Compression.FileSystem
   $CompressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($Source, $Filename, $CompressionLevel, $false)
}

$BuildPath = "$PSScriptRoot\GoToWindow\bin\Release"

If(Test-Path -Path $BuildPath) {
	Remove-Item -Confirm:$false "$BuildPath\*.*"
}

&(GetMSBuildExe) GoToWindow.sln `
	/t:Clean`;Rebuild `
	/p:Configuration=Release `
	/p:AllowedReferenceRelatedFileExtensions=- `
	/p:DebugSymbols=false `
	/p:DebugType=None `
	/clp:ErrorsOnly `
	/v:m

$ReleaseFolder = "$PSScriptRoot\Release"
$ReleaseZip = "$ReleaseFolder\GoToWindow.zip"
	
If(!(Test-Path -Path $ReleaseFolder )){
    New-Item -ItemType directory -Path $ReleaseFolder
}

If(Test-Path -Path $ReleaseZip) {
	Remove-Item -Confirm:$false .\Release\GoToWindow.zip
}

ZipFiles $ReleaseZip $BuildPath