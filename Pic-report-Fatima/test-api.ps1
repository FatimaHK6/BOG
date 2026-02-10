[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

$ProgressPreference = 'SilentlyContinue'
try {
    $response = Invoke-WebRequest -Uri 'http://localhost:5000/api/lookups/case-types' -TimeoutSec 10
    Write-Host "Status: $($response.StatusCode)"
    Write-Host "Content length: $($response.Content.Length)"
    Write-Host "Full Response: $($response.Content)"
} catch {
    Write-Host "HTTP Status: $($_.Exception.Response.StatusCode)"
    Write-Host "Error Message: $($_.Exception.Message)"

    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = [System.IO.StreamReader]::new($stream)
        $body = $reader.ReadToEnd()
        Write-Host "Response Body: $body"
    }
}
