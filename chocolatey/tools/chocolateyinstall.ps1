$packageName = 'GoToWindow'
$url        = 'https://github.com/christianrondeau/GoToWindow/releases/download/v0.6.1/GoToWindow.Setup.0.6.1.exe'
$installerType = 'exe'
$silentArgs = '--silent'

Install-ChocolateyPackage $packageName $installerType $silentArgs $url
