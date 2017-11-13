$packageArgs = @{
  packageName    = 'GoToWindow'
  installerType  = 'exe'
  url            = 'https://github.com/christianrondeau/GoToWindow/releases/download/v0.7.3/GoToWindow.Setup.0.7.3.exe'
  checksumType   = 'sha256'
  checksum       = '2CB16CBE99C66F3335578458A8D64354EF225DEB4036EF0FC6DE9BDAA8F78069'
  silentArgs     = '--silent'
  validExitCodes = @(0)
}

Install-ChocolateyPackage @packageArgs
