$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = "test-case"
$watcher.Filter = "*.*"
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite


$testall =
{
	Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "test-all\PicoPDF.TestAll.csproj", "--no-launch-profile" -WorkingDirectory "." -NoNewWindow -Wait
	Write-Host "$(Get-Date) dotnet test-all done"
}

$action =
{
	$exten = [System.IO.Path]::GetExtension($Event.SourceEventArgs.FullPath)
	if ($exten -ne ".pdf")
	{
		Invoke-Command $testall
	}
}

Register-ObjectEvent $watcher "Changed" -Action $action
Invoke-Command $testall

while ($true)
{
	Start-Sleep 1
}
