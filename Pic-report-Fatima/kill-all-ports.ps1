$ports = 5000, 5001
foreach ($port in $ports) {
    Write-Host "Killing processes on port $port..."
    Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Found process: $($_.OwningProcess)"
        Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
    }
}
Start-Sleep -Seconds 3

foreach ($port in $ports) {
    Write-Host "Checking port $port..."
    $connections = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($connections) {
        Write-Host "Still in use: $($connections.OwningProcess)"
    } else {
        Write-Host "Port $port is free"
    }
}
