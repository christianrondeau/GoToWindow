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
$Squirrel = Join-Path  (ls .\packages\squirrel.windows.*)[0] "tools\Squirrel.com"

$BuildPath = "$PSScriptRoot\GoToWindow\bin\Release"
$NuSpecPath = "$PSScriptRoot\GoToWindow.nuspec"
$ReleasesFolder = "$PSScriptRoot\Releases"

$SetupLoadingGif = "$PSScriptRoot\GoToWindow.Setup\Loading.gif"

# ==================================== NuSpec Metadata

$NuSpecXml = [xml](Get-Content $NuSpecPath)
$Version = $NuSpecXml.package.metadata.version

# ==================================== Synchronize RELEASES

git checkout Releases\RELEASES

$ReleasesToDownload = (Get-Content "$ReleasesFolder\RELEASES") | % { ($_ -split ' ')[1] }
$ReleaseFilenames = @()

$WebClient = New-Object System.Net.WebClient

$ReleasesToDownload | % {
	$ReleaseFilename = $_.Split("/")[-1]
	$ReleaseFilenames += $ReleaseFilename
	$ReleasePath = "$ReleasesFolder\$ReleaseFilename"

	If(Test-Path -Path $ReleasePath) {
		Write-Host "$ReleaseFilename already present" -ForegroundColor Gray
	} Else {
		Write-Host "Downloading $ReleaseFilename..."
		$WebClient.DownloadFile($_, $ReleasePath)
		Write-Host "$ReleaseFilename downloaded"
	}
}

Write-Host "Deleting unreferenced nupkg files"
dir "$ReleasesFolder\*.nupkg" | ? { $ReleaseFilenames -NotContains $_.Name } | Remove-Item

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

&($NuGet) pack $NuSpecPath

$SquirrelFullNuPkgOutputPath = "$PSScriptRoot\Releases\GoToWindow-$Version-full.nupkg"
If(Test-Path -Path $SquirrelFullNuPkgOutputPath) {
	Remove-Item -Confirm:$false $SquirrelFullNuPkgOutputPath
}

$SquirrelDeltaNuPkgOutputPath = "$PSScriptRoot\Releases\GoToWindow-$Version-delta.nupkg"
If(Test-Path -Path $SquirrelDeltaNuPkgOutputPath) {
	Remove-Item -Confirm:$false $SquirrelDeltaNuPkgOutputPath
}

$OutputSetupExe = "$ReleasesFolder\GoToWindow.Setup.$Version.exe"
If(Test-Path -Path $OutputSetupExe) {
	Remove-Item -Confirm:$false $OutputSetupExe
}

&($Squirrel) -g $SetupLoadingGif --releasify $NuPkgPath -baseUrl https://github.com/christianrondeau/GoToWindow/releases/download/v$Version/

$SquirrelSetupExe = "$ReleasesFolder\Setup.exe"
If(Test-Path -Path $SquirrelSetupExe) {
	Rename-Item $SquirrelSetupExe $OutputSetupExe
}

# ==================================== Cleanup

If(Test-Path -Path $NuPkgPath) {
	Remove-Item -Confirm:$false $NuPkgPath
}

# ==================================== Complete

Write-Host "Build $Version complete: $ReleasesFolder" -ForegroundColor Green
