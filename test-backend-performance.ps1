# Backend Performance Comparison Script
# Compares Azure vs Railway performance

param(
    [int]$Iterations = 10,
    [string]$Token = ""  # JWT token for authenticated endpoints
)

$AzureUrl = "https://api.test1hotelwebsite.online"
$RailwayUrl = "https://websitebuilderapi-production-production.up.railway.app"

# Test endpoints
$endpoints = @(
    "/api/health",
    "/api/company/1",
    "/api/products",
    "/api/rooms",
    "/api/navigation-menus/company/1"
)

# Results storage
$results = @{
    Azure = @()
    Railway = @()
}

Write-Host "🚀 Starting Backend Performance Comparison" -ForegroundColor Cyan
Write-Host "Iterations: $Iterations per endpoint" -ForegroundColor Yellow
Write-Host ""

function Test-Endpoint {
    param(
        [string]$BaseUrl,
        [string]$Endpoint,
        [string]$Token
    )
    
    $headers = @{}
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }
    
    try {
        $response = Measure-Command {
            Invoke-RestMethod -Uri "$BaseUrl$Endpoint" -Headers $headers -TimeoutSec 30
        }
        return $response.TotalMilliseconds
    }
    catch {
        Write-Host "❌ Error testing $BaseUrl$Endpoint" -ForegroundColor Red
        return -1
    }
}

# Test each backend
foreach ($endpoint in $endpoints) {
    Write-Host "Testing endpoint: $endpoint" -ForegroundColor Green
    
    # Test Azure
    Write-Host "  Azure: " -NoNewline
    $azureTimes = @()
    for ($i = 1; $i -le $Iterations; $i++) {
        $time = Test-Endpoint -BaseUrl $AzureUrl -Endpoint $endpoint -Token $Token
        if ($time -gt 0) {
            $azureTimes += $time
            Write-Host "." -NoNewline -ForegroundColor Blue
        }
        Start-Sleep -Milliseconds 100
    }
    $avgAzure = ($azureTimes | Measure-Object -Average).Average
    Write-Host " Avg: $([math]::Round($avgAzure, 2))ms" -ForegroundColor Blue
    
    # Test Railway
    Write-Host "  Railway: " -NoNewline
    $railwayTimes = @()
    for ($i = 1; $i -le $Iterations; $i++) {
        $time = Test-Endpoint -BaseUrl $RailwayUrl -Endpoint $endpoint -Token $Token
        if ($time -gt 0) {
            $railwayTimes += $time
            Write-Host "." -NoNewline -ForegroundColor Magenta
        }
        Start-Sleep -Milliseconds 100
    }
    $avgRailway = ($railwayTimes | Measure-Object -Average).Average
    Write-Host " Avg: $([math]::Round($avgRailway, 2))ms" -ForegroundColor Magenta
    
    # Store results
    $results.Azure += @{
        Endpoint = $endpoint
        Average = $avgAzure
        Min = ($azureTimes | Measure-Object -Minimum).Minimum
        Max = ($azureTimes | Measure-Object -Maximum).Maximum
    }
    
    $results.Railway += @{
        Endpoint = $endpoint
        Average = $avgRailway
        Min = ($railwayTimes | Measure-Object -Minimum).Minimum
        Max = ($railwayTimes | Measure-Object -Maximum).Maximum
    }
    
    # Compare
    $diff = $avgRailway - $avgAzure
    $percentDiff = ($diff / $avgAzure) * 100
    
    if ($diff -lt 0) {
        Write-Host "  ✅ Railway is $([math]::Round([math]::Abs($diff), 2))ms faster ($([math]::Round([math]::Abs($percentDiff), 1))%)" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ Azure is $([math]::Round($diff, 2))ms faster ($([math]::Round($percentDiff, 1))%)" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Summary
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "📊 PERFORMANCE SUMMARY" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan

$azureAvgTotal = ($results.Azure.Average | Measure-Object -Average).Average
$railwayAvgTotal = ($results.Railway.Average | Measure-Object -Average).Average

Write-Host ""
Write-Host "Azure Average Response Time: $([math]::Round($azureAvgTotal, 2))ms" -ForegroundColor Blue
Write-Host "Railway Average Response Time: $([math]::Round($railwayAvgTotal, 2))ms" -ForegroundColor Magenta
Write-Host ""

if ($railwayAvgTotal -lt $azureAvgTotal) {
    $improvement = (($azureAvgTotal - $railwayAvgTotal) / $azureAvgTotal) * 100
    Write-Host "🏆 WINNER: Railway is $([math]::Round($improvement, 1))% faster overall!" -ForegroundColor Green
} else {
    $improvement = (($railwayAvgTotal - $azureAvgTotal) / $railwayAvgTotal) * 100
    Write-Host "🏆 WINNER: Azure is $([math]::Round($improvement, 1))% faster overall!" -ForegroundColor Blue
}

# Save results to file
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$results | ConvertTo-Json -Depth 3 | Out-File "performance-comparison-$timestamp.json"
Write-Host ""
Write-Host "Results saved to: performance-comparison-$timestamp.json" -ForegroundColor Gray