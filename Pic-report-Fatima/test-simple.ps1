$url = "http://localhost:5000/api/lookups/case-types"
$ProgressPreference = "SilentlyContinue"

try {
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing
    "Status Code: $($response.StatusCode)"
    "Response Length: $($response.Content.Length)"
    "Response Content:"
    $response.Content | ConvertFrom-Json | ConvertTo-Json
} catch {
    "Error: $($_.Exception.Message)"
}
