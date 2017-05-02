$localApplicationData = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::LocalApplicationData))
$gotowindowFolder = Join-Path $localApplicationData "GoToWindow"
$updateExe = Join-Path $gotowindowFolder "Update.exe"

Start-Process -FilePath $updateExe -ArgumentList "--uninstall" -Verb "RunAs" -Wait
