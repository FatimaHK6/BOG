#!/usr/bin/env pwsh
# Kill any existing API instances
Get-Process | Where-Object { $_.ProcessName -eq "BOG.API" -or $_.ProcessName -eq "dotnet" } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Start the API
Write-Host "Starting BOG.API..."
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "C:\Users\Lenovo\Desktop\Claude\BOG\src\Backend\BOG.API" -PassThru

# Wait for API to start
Write-Host "Waiting for API to be ready..."
$maxAttempts = 30
$attempt = 0
while ($attempt -lt $maxAttempts) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5001/api/lookups/attachment-types" -ErrorAction Stop
        Write-Host "API is ready!"
        Write-Host "Attachment Types Response:"
        Write-Host $response.Content
        break
    }
    catch {
        $attempt++
        Write-Host "Attempt $attempt/$maxAttempts: API not ready yet..."
        Start-Sleep -Seconds 1
    }
}

# Cleanup
Stop-Process -InputObject $apiProcess -Force -ErrorAction SilentlyContinue
