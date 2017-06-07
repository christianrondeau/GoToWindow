[CmdletBinding()]
Param(
	[String][Parameter(Mandatory=$True)]$Version
)

Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Updating version number to $Version" -ForegroundColor Green

Function AssignVersionToFile {
	[CmdletBinding()]
	Param (
		[String][Parameter(Mandatory=$True)]$Path,
		[String][Parameter(Mandatory=$True)]$RegEx,
		[String][Parameter(Mandatory=$True)]$Replacement
	)
	
	(Get-Content $Path) -Replace $RegEx, $Replacement | Out-File $Path -Encoding UTF8
}

AssignVersionToFile -Path "$PSScriptRoot\GoToWindow.nuspec" -RegEx "<version>[^<]+</version>" -Replacement "<version>$($Version)</version>"
AssignVersionToFile -Path "$PSScriptRoot\GoToWindow.Shared\Properties\AssemblyInfo.Shared.cs" -RegEx "`"(\d+\.\d+\.\d+)`"" -Replacement "`"$($Version)`""
AssignVersionToFile -Path "$PSScriptRoot\chocolatey\gotowindow.nuspec" -RegEx "<version>[^<]+</version>"-Replacement "<version>$($Version)</version>"
AssignVersionToFile -Path "$PSScriptRoot\chocolatey\tools\chocolateyinstall.ps1" -RegEx "(\d+\.\d+\.\d+)" -Replacement "$($Version)"

Write-Host "Version updated!" -ForegroundColor Green
