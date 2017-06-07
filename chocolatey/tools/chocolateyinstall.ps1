$packageArgs = @{
  packageName    = 'GoToWindow'
  installerType  = 'exe'
  url            = 'https://github.com/christianrondeau/GoToWindow/releases/download/v0.7.2/GoToWindow.Setup.0.7.2.exe'
  checksumType   = 'sha256'
  checksum       = '1AC20D3225DA6135FFAA943E7A7F0D8F95923DDF3E7B019EE0E09CD77CE49A78'
  silentArgs     = '--silent'
  validExitCodes = @(0)
}

Install-ChocolateyPackage @packageArgs
