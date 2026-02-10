[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
$response = Invoke-WebRequest -Uri 'https://localhost:5001/api/lookups/courts' -SkipCertificateCheck
Write-Host $response.Content
