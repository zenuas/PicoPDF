$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = "test-case"
$watcher.Filter = "*.*"
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite

$watcher2 = New-Object System.IO.FileSystemWatcher
$watcher2.Path = "."
$watcher2.Filter = "@b.bmp"
$watcher2.IncludeSubdirectories = $true
$watcher2.NotifyFilter = [System.IO.NotifyFilters]::LastWrite


$action =
{
	Start-Process -FilePath "sample/bin/Debug/net7.0/PicoPDF.Sample.exe" -WorkingDirectory "." -NoNewWindow -Wait
	Write-Host "$(Get-Date) PicoPDF.Sample.exe done"
}

$action2 =
{
	Start-Process -FilePath "cmd" -ArgumentList "/c", "start", "@b.bmp" -WorkingDirectory "." -NoNewWindow
	Write-Host "$(Get-Date) start @b.bmp done"
}

Register-ObjectEvent $watcher  "Changed" -Action $action
Register-ObjectEvent $watcher2 "Changed" -Action $action2
Invoke-Command $action

while ($true)
{
	Start-Sleep 1
}
