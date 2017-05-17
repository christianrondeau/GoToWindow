$packageName = 'GoToWindow'
$url        = 'https://github.com/christianrondeau/GoToWindow/releases/download/v0.7.0/GoToWindow.Setup.0.7.0.exe'
$installerType = 'exe'
$silentArgs = '--silent'

Install-ChocolateyPackage $packageName $installerType $silentArgs $url
