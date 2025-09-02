# Test Railway API directly
$headers = @{
    "Authorization" = "Bearer 03d0d440-ec46-40eb-af4b-dd1952c443c1"
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri "https://backboard.railway.app/graphql/v2" -Method POST -Headers $headers -Body '{"query":"{ me { id email } }"}'
    Write-Host "API Response:" -ForegroundColor Green
    $response | ConvertTo-Json
} catch {
    Write-Host "API Error:" -ForegroundColor Red
    $_.Exception.Message
}