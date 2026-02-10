# Test script to create a complete case request with all required data

$apiUrl = "https://localhost:5001/api"

# Ignore SSL certificate validation for localhost
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

Write-Host "Creating test case request..."

# 1. Create case request
$createPayload = @{
    courtId = 1
    subject = "اختبار شامل لنظام إدارة الدعاوى - قضية اختبار للتحقق من جميع المتطلبات"
    evidence = "الأدلة والمستندات المرفقة تشمل: صور الهوية، العقود، والمخاطبات الرسمية. هذا الطلب يهدف للتحقق من اكتمال النظام وجاهزيته للعمل الفعلي."
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$apiUrl/case-requests" `
        -Method POST `
        -ContentType "application/json" `
        -Body $createPayload `
        -SkipCertificateCheck
    
    $request = $response.Content | ConvertFrom-Json
    $requestId = $request.id
    
    Write-Host "✓ Request created: ID = $requestId"
    Write-Host ""
    
    # 2. Add defendant
    Write-Host "Adding defendant..."
    $defendantPayload = @{
        defendantTypeId = 1
        fullName = "محمد أحمد علي الشمري"
        identityTypeId = 1
        identityNumber = "1234567890"
        addressText = "الرياض - حي النخيل - شارع الملك فهد"
        additionalStatement = "مدعى عليه في قضية تجارية"
    } | ConvertTo-Json
    
    $defendantResponse = Invoke-WebRequest -Uri "$apiUrl/case-requests/$requestId/defendants" `
        -Method POST `
        -ContentType "application/json" `
        -Body $defendantPayload `
        -SkipCertificateCheck
    
    $defendant = $defendantResponse.Content | ConvertFrom-Json
    Write-Host "✓ Defendant added: ID = $($defendant.id)"
    Write-Host ""
    
    Write-Host "Test request created successfully!"
    Write-Host "Request ID: $requestId"
    Write-Host ""
    Write-Host "Next steps:"
    Write-Host "1. Open: http://localhost:4200/case-registration/$requestId/edit"
    Write-Host "2. Add classification in 'معلومات إضافية' tab"
    Write-Host "3. Upload attachments in 'المرفقات' tab"
    Write-Host "4. Click 'إرسال الطلب' in 'إجراءات الطلب' tab"
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception.Response.Content
}
