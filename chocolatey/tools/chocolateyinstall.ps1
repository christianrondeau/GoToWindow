$packageArgs = @{
  packageName    = 'GoToWindow'
  installerType  = 'exe'
  url            = 'https://github.com/christianrondeau/GoToWindow/releases/download/v0.7.2/GoToWindow.Setup.0.7.2.exe'
  checksumType   = 'sha256'
  checksum       = '2073F863199E32C7A5DAD06D2004280A0E316A7686ED8496F3FC29854926CEC8'
  silentArgs     = '--silent'
  validExitCodes = @(0)
}

Install-ChocolateyPackage @packageArgs
