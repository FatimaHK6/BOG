Get-NetTCPConnection -LocalPort 5000 -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Killing process $($_.OwningProcess)"
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2
Get-NetTCPConnection -LocalPort 5000 -State Listen -ErrorAction SilentlyContinue | Format-Table OwningProcess, State
