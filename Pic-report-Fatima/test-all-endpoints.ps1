$ProgressPreference = "SilentlyContinue"
$baseUrl = "http://localhost:5000"

$endpoints = @(
    "/api/lookups/case-types",
    "/api/lookups/classifications",
    "/api/lookups/attachment-types",
    "/api/lookups/courts"
)

foreach ($endpoint in $endpoints) {
    $url = "$baseUrl$endpoint"
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing
        $count = ($response.Content | ConvertFrom-Json).value.Count
        Write-Host "OK: $endpoint - Status $($response.StatusCode), Items: $count"
    } catch {
        Write-Host "ERROR: $endpoint - $($_.Exception.Message)"
    }
}
