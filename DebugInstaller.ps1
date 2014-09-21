.\CleanUpInstalls.ps1; ri .\Releases\* -Force; .\ChangeVersion.ps1 -Version 0.1.7; .\Build.ps1; .\Releases\Setup.exe; .\ChangeVersion.ps1 -Version 0.1.8; .\Build.ps1
