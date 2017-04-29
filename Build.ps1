Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Building Go To Window..." -ForegroundColor Green

# ==================================== Functions

Function GetMSBuildExe {
	Return "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
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
$SetupIcon = "$PSScriptRoot\GoToWindow\Resources\AppIcon.ico"

# ==================================== NuSpec Metadata

$NuSpecXml = [xml](Get-Content $NuSpecPath)
$Version = $NuSpecXml.package.metadata.version

# ==================================== Synchronize RELEASES

Write-Host "Synchronizing releases..." -ForegroundColor White

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

Write-Host "Building..." -ForegroundColor White

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

Write-Host "Zipping..." -ForegroundColor White

$ReleaseZip = "$ReleasesFolder\GoToWindow.$Version.zip"
	
If(!(Test-Path -Path $ReleasesFolder )){
    New-Item -ItemType directory -Path $ReleasesFolder
}

If(Test-Path -Path $ReleaseZip) {
	Remove-Item -Confirm:$false $ReleaseZip
}

ZipFiles $ReleaseZip $BuildPath

# ==================================== Squirrel

Write-Host "Squirrel..." -ForegroundColor White

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

$OutputSetupMsi = "$ReleasesFolder\GoToWindow.Setup.$Version.msi"
If(Test-Path -Path $OutputSetupMsi) {
	Remove-Item -Confirm:$false $OutputSetupMsi
}

&($Squirrel) -g $SetupLoadingGif --releasify $NuPkgPath -i $SetupIcon -baseUrl https://github.com/christianrondeau/GoToWindow/releases/download/v$Version/

$SquirrelSetupMsi = "$ReleasesFolder\Setup.msi"
If(Test-Path -Path $SquirrelSetupMsi) {
	Rename-Item $SquirrelSetupMsi $OutputSetupMsi
}

# ==================================== Cleanup

Write-Host "Cleanup..." -ForegroundColor White

If(Test-Path -Path $NuPkgPath) {
	Remove-Item -Confirm:$false $NuPkgPath
}

# ==================================== Complete

Write-Host "Build $Version complete: $ReleasesFolder" -ForegroundColor Green
