$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = "test-case"
$watcher.Filter = "*.*"
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter          = [System.IO.NotifyFilters]::LastAccess

$action =
{
	Start-Process -FilePath "sample/bin/Debug/net7.0/PicoPDF.Sample.exe" -WorkingDirectory "." -NoNewWindow -Wait
	Write-Host "$(Get-Date) done"
}

Register-ObjectEvent $watcher "Changed" -Action $action
Invoke-Command $action

while ($true)
{
	Start-Sleep 1
}
