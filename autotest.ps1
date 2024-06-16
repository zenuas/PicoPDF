$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = "test-case"
$watcher.Filter = "*.*"
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite


function Test-All($always_update)
{
	Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "test-all\PicoPDF.TestAll.csproj", "--no-launch-profile", "--", "--always-update", $always_update -WorkingDirectory "." -NoNewWindow -Wait
	Write-Host "$(Get-Date) dotnet test-all done"
}

$action =
{
	$exten = [System.IO.Path]::GetExtension($Event.SourceEventArgs.FullPath)
	if ($exten -ne ".pdf")
	{
		Test-All false
	}
}

Register-ObjectEvent $watcher "Changed" -Action $action
Test-All true

while ($true)
{
	Start-Sleep 1
}
