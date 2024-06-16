$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = "test-case"
$watcher.Filter = "*.*"
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite

$watcher2 = New-Object System.IO.FileSystemWatcher
$watcher2.Path = "test-all\bin"
$watcher2.Filter = "*.*"
$watcher2.IncludeSubdirectories = $true
$watcher2.NotifyFilter = [System.IO.NotifyFilters]::LastWrite


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

$lastaction2 = Get-Date
$action2 =
{
	$exten = [System.IO.Path]::GetExtension($Event.SourceEventArgs.FullPath)
	if ($lastaction2 -lt ((Get-Date).AddSeconds(-5)) -and ($exten -eq ".exe" -or $exten -eq ".dll"))
	{
		$lastaction2 = Get-Date
		Test-All true
	}
}

Register-ObjectEvent $watcher  "Changed" -Action $action
Register-ObjectEvent $watcher2 "Changed" -Action $action2
Test-All true

while ($true)
{
	Start-Sleep 1
}
