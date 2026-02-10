[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

$ProgressPreference = 'SilentlyContinue'
$url = 'http://localhost:5000/api/lookups/case-types'

Write-Host "Testing URL: $url"

$retryCount = 0
$maxRetries = 3

while ($retryCount -lt $maxRetries) {
    try {
        Write-Host "Attempt $($retryCount + 1)..."
        $response = Invoke-WebRequest -Uri $url -TimeoutSec 10 -ErrorAction Stop
        Write-Host "Status Code: $($response.StatusCode)"
        Write-Host "Content Type: $($response.Headers['Content-Type'])"
        Write-Host "Response Length: $($response.Content.Length)"
        Write-Host "Response Content:"
        Write-Host "$($response.Content)"
        break
    } catch [System.Net.Http.HttpRequestException] {
        Write-Host "HttpRequestException: $($_.Exception.Message)"
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Host "Retrying in 2 seconds..."
            Start-Sleep -Seconds 2
        }
    } catch [System.Net.WebException] {
        $ex = $_.Exception
        Write-Host "WebException: $($ex.Message)"
        if ($ex.Response) {
            try {
                $stream = $ex.Response.GetResponseStream()
                $reader = [System.IO.StreamReader]::new($stream)
                $body = $reader.ReadToEnd()
                Write-Host "Response Body: $body"
            } catch {
                Write-Host "Could not read response body"
            }
        }
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Host "Retrying in 2 seconds..."
            Start-Sleep -Seconds 2
        }
    } catch {
        Write-Host "General Exception: $($_.Exception.GetType().FullName)"
        Write-Host "Message: $($_.Exception.Message)"
        if ($_.Exception.InnerException) {
            Write-Host "Inner Exception: $($_.Exception.InnerException.Message)"
        }
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Host "Retrying in 2 seconds..."
            Start-Sleep -Seconds 2
        }
    }
}
