[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

$ProgressPreference = 'SilentlyContinue'
try {
    $response = Invoke-WebRequest -Uri 'http://localhost:5000/swagger/index.html' -TimeoutSec 10
    Write-Host "Swagger Status: $($response.StatusCode)"
    if ($response.StatusCode -eq 200) {
        Write-Host "Swagger UI is accessible!"
    }
} catch {
    Write-Host "Swagger Error: $($_.Exception.Message)"
}
