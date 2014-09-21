Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Building Go To Window..." -ForegroundColor Green

# ==================================== Functions

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

# ==================================== Variables

$NuGet = "$PSScriptRoot\.nuget\NuGet.exe"
$Squirrel = "$PSScriptRoot\packages\squirrel.windows.0.4.94\tools\Squirrel.com"

$BuildPath = "$PSScriptRoot\GoToWindow\bin\Release"
$NuSpecPath = "$PSScriptRoot\GoToWindow.nuspec"
$ReleasesFolder = "$PSScriptRoot\Releases"

# ==================================== NuSpec Metadata

$NuSpecXml = [xml](Get-Content $NuSpecPath)
$Version = $NuSpecXml.package.metadata.version

# ==================================== Build

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

# ==================================== Zip

$ReleaseZip = "$ReleasesFolder\GoToWindow.$Version.zip"
	
If(!(Test-Path -Path $ReleasesFolder )){
    New-Item -ItemType directory -Path $ReleasesFolder
}

If(Test-Path -Path $ReleaseZip) {
	Remove-Item -Confirm:$false $ReleaseZip
}

ZipFiles $ReleaseZip $BuildPath

# ==================================== Squirrel

$NuPkgPath = "$PSScriptRoot\GoToWindow.$Version.nupkg"
$SquirrelNuPkgOutputPath = "$PSScriptRoot\Releases\GoToWindow.$Version.nupkg"

&($NuGet) pack $NuSpecPath

If(Test-Path -Path $SquirrelNuPkgOutputPath) {
	Remove-Item -Confirm:$false $SquirrelNuPkgOutputPath
}

&($Squirrel) --releasify $NuPkgPath

# ==================================== Cleanup

If(Test-Path -Path $NuPkgPath) {
	Remove-Item -Confirm:$false $NuPkgPath
}

# ==================================== Complete

Write-Host "Build complete!" -ForegroundColor Green
