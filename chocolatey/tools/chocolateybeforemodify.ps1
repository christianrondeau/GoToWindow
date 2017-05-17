$Running = Get-Process GoToWindow -ErrorAction SilentlyContinue

if ($Running)
{
	Write-Host "Stopping GoToWindow running instances."
		foreach($Process in $Running)
		{
			Stop-Process $Process
		}
}
