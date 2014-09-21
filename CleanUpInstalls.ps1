Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Cleaning up Squirrel installs..." -ForegroundColor Green

Remove-Item "$env:LOCALAPPDATA\GoToWindow" -Force -Recurse

Write-Host "Cleaning up complete" -ForegroundColor Green
