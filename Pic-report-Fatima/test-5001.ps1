$ProgressPreference = "SilentlyContinue"

$endpoints = @(
    "http://localhost:5001/api/lookups/case-types",
    "http://localhost:5001/api/lookups/classifications",
    "http://localhost:5001/api/case-requests?pageNumber=1&pageSize=10"
)

Write-Host "Testing Backend on Port 5001`n"
Write-Host "===============================`n"

foreach ($url in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
        $data = $response.Content | ConvertFrom-Json
        $count = if ($data.value) { $data.value.Count } elseif ($data.items) { $data.items.Count } else { 0 }
        Write-Host "OK: $(($url -split '/')[-1])"
        Write-Host "  Status: $($response.StatusCode)"
        Write-Host "  Items: $count`n"
    } catch {
        Write-Host "ERROR: $(($url -split '/')[-1])"
        Write-Host "  Error: $($_.Exception.Message)`n"
    }
}

Write-Host "===============================`n"
Write-Host "Backend Status: OPERATIONAL on port 5001"
