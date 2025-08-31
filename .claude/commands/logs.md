# Logs Analysis Command

View, search, and analyze WebsiteBuilder API logs: $ARGUMENTS

## Overview
This command provides comprehensive log analysis for both backend (ASP.NET Core with Serilog) and frontend (Next.js) logs, following the system described in logs.md.

## Step 1: Check Today's Logs

View the current day's log file:

```bash
# Get today's date in the format used by logs
$today = Get-Date -Format "yyyyMMdd"
$logPath = "C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\websitebuilder-$today.txt"

Write-Host "📊 Checking logs for: $(Get-Date -Format 'yyyy-MM-dd')" -ForegroundColor Cyan
Write-Host "Log file: $logPath" -ForegroundColor Gray

if (Test-Path $logPath) {
    $lineCount = (Get-Content $logPath | Measure-Object -Line).Lines
    Write-Host "✅ Log file found with $lineCount lines" -ForegroundColor Green
} else {
    Write-Host "⚠️ No log file found for today" -ForegroundColor Yellow
    Write-Host "Make sure the backend is running with: dotnet run" -ForegroundColor Gray
}
```

## Step 2: View Recent Logs (Last 50 Lines)

Display the most recent log entries:

```bash
powershell.exe -Command "
\$today = Get-Date -Format 'yyyyMMdd'
\$logPath = \"C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\websitebuilder-\$today.txt\"

if (Test-Path \$logPath) {
    Write-Host ''
    Write-Host '📜 LAST 50 LOG ENTRIES:' -ForegroundColor Cyan
    Write-Host '========================' -ForegroundColor Gray
    Get-Content \$logPath -Tail 50 | ForEach-Object {
        if (\$_ -match '\[ERR\]|\bError\b|\bFailed\b') {
            Write-Host \$_ -ForegroundColor Red
        } elseif (\$_ -match '\[WRN\]|\bWarning\b|\bSlow\b') {
            Write-Host \$_ -ForegroundColor Yellow
        } elseif (\$_ -match '\[INF\]') {
            Write-Host \$_ -ForegroundColor Green
        } elseif (\$_ -match '\[DBG\]') {
            Write-Host \$_ -ForegroundColor Gray
        } elseif (\$_ -match 'Frontend Error:') {
            Write-Host \$_ -ForegroundColor Magenta
        } elseif (\$_ -match 'Frontend Warning:') {
            Write-Host \$_ -ForegroundColor DarkYellow
        } elseif (\$_ -match 'Frontend Log:') {
            Write-Host \$_ -ForegroundColor Cyan
        } else {
            Write-Host \$_
        }
    }
} else {
    Write-Host '❌ No log file found for today' -ForegroundColor Red
}
"
```

## Step 3: Search for Errors

Find all errors in today's logs:

```bash
powershell.exe -Command "
\$today = Get-Date -Format 'yyyyMMdd'
\$logPath = \"C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\websitebuilder-\$today.txt\"

if (Test-Path \$logPath) {
    Write-Host ''
    Write-Host '🔴 ERRORS FOUND:' -ForegroundColor Red
    Write-Host '=================' -ForegroundColor Gray
    
    \$errors = Select-String -Path \$logPath -Pattern '\[ERR\]|Error:|Exception:|Failed:' -Context 0,2
    
    if (\$errors) {
        \$errorCount = \$errors.Count
        Write-Host \"Found \$errorCount error(s)\" -ForegroundColor Yellow
        Write-Host ''
        
        \$errors | ForEach-Object {
            Write-Host \"Line \$(\$_.LineNumber): \" -NoNewline -ForegroundColor Gray
            Write-Host \$_.Line -ForegroundColor Red
            if (\$_.Context.PostContext) {
                \$_.Context.PostContext | ForEach-Object {
                    Write-Host \"  \$_\" -ForegroundColor DarkRed
                }
            }
            Write-Host ''
        }
    } else {
        Write-Host '✅ No errors found!' -ForegroundColor Green
    }
} else {
    Write-Host '❌ No log file found' -ForegroundColor Red
}
"
```

## Step 4: Search for Frontend Logs

Find all frontend-related logs:

```bash
powershell.exe -Command "
\$today = Get-Date -Format 'yyyyMMdd'
\$logPath = \"C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\websitebuilder-\$today.txt\"

if (Test-Path \$logPath) {
    Write-Host ''
    Write-Host '🌐 FRONTEND LOGS:' -ForegroundColor Cyan
    Write-Host '==================' -ForegroundColor Gray
    
    \$frontendLogs = Select-String -Path \$logPath -Pattern 'Frontend Log:|Frontend Error:|Frontend Warning:' -Context 0,1
    
    if (\$frontendLogs) {
        \$logCount = \$frontendLogs.Count
        Write-Host \"Found \$logCount frontend log(s)\" -ForegroundColor Yellow
        Write-Host ''
        
        \$frontendLogs | Select-Object -Last 20 | ForEach-Object {
            \$line = \$_.Line
            if (\$line -match 'Frontend Error:') {
                Write-Host \$line -ForegroundColor Red
            } elseif (\$line -match 'Frontend Warning:') {
                Write-Host \$line -ForegroundColor Yellow
            } else {
                Write-Host \$line -ForegroundColor Cyan
            }
        }
    } else {
        Write-Host '⚠️ No frontend logs found' -ForegroundColor Yellow
        Write-Host 'Make sure LoggerInitializer is active in the frontend' -ForegroundColor Gray
    }
} else {
    Write-Host '❌ No log file found' -ForegroundColor Red
}
"
```

## Step 5: Monitor Slow Queries

Find slow database queries:

```bash
powershell.exe -Command "
\$today = Get-Date -Format 'yyyyMMdd'
\$logPath = \"C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\websitebuilder-\$today.txt\"

if (Test-Path \$logPath) {
    Write-Host ''
    Write-Host '🐌 SLOW QUERIES:' -ForegroundColor Yellow
    Write-Host '=================' -ForegroundColor Gray
    
    \$slowQueries = Select-String -Path \$logPath -Pattern 'Slow query|took \d{3,}ms|exceeded.*ms' -Context 1,1
    
    if (\$slowQueries) {
        \$queryCount = \$slowQueries.Count
        Write-Host \"Found \$queryCount slow query(ies)\" -ForegroundColor Yellow
        Write-Host ''
        
        \$slowQueries | ForEach-Object {
            Write-Host \"Line \$(\$_.LineNumber):\" -ForegroundColor Gray
            if (\$_.Context.PreContext) {
                Write-Host \"  \$(\$_.Context.PreContext)\" -ForegroundColor DarkYellow
            }
            Write-Host \"  \$(\$_.Line)\" -ForegroundColor Yellow
            if (\$_.Context.PostContext) {
                Write-Host \"  \$(\$_.Context.PostContext)\" -ForegroundColor DarkYellow
            }
            Write-Host ''
        }
    } else {
        Write-Host '✅ No slow queries detected!' -ForegroundColor Green
    }
} else {
    Write-Host '❌ No log file found' -ForegroundColor Red
}
"
```

## Step 6: API Performance Analysis

Analyze API response times:

```bash
powershell.exe -Command "
\$today = Get-Date -Format 'yyyyMMdd'
\$logPath = \"C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\websitebuilder-\$today.txt\"

if (Test-Path \$logPath) {
    Write-Host ''
    Write-Host '⚡ API PERFORMANCE:' -ForegroundColor Cyan
    Write-Host '===================' -ForegroundColor Gray
    
    \$apiCalls = Select-String -Path \$logPath -Pattern 'HTTP (GET|POST|PUT|DELETE|PATCH).*responded \d+ in [\d\.]+ ms'
    
    if (\$apiCalls) {
        # Extract response times
        \$times = @()
        \$apiCalls | ForEach-Object {
            if (\$_.Line -match 'in ([\d\.]+) ms') {
                \$times += [double]\$matches[1]
            }
        }
        
        if (\$times.Count -gt 0) {
            \$avg = (\$times | Measure-Object -Average).Average
            \$max = (\$times | Measure-Object -Maximum).Maximum
            \$min = (\$times | Measure-Object -Minimum).Minimum
            
            Write-Host \"Total API calls: \$(\$apiCalls.Count)\" -ForegroundColor White
            Write-Host \"Average response time: \$([math]::Round(\$avg, 2))ms\" -ForegroundColor Green
            Write-Host \"Fastest response: \$([math]::Round(\$min, 2))ms\" -ForegroundColor Green
            Write-Host \"Slowest response: \$([math]::Round(\$max, 2))ms\" -ForegroundColor Yellow
            
            Write-Host ''
            Write-Host 'Slowest 5 endpoints:' -ForegroundColor Yellow
            \$apiCalls | ForEach-Object {
                if (\$_.Line -match '(HTTP \w+ [^\s]+).*in ([\d\.]+) ms') {
                    [PSCustomObject]@{
                        Endpoint = \$matches[1]
                        Time = [double]\$matches[2]
                        Line = \$_.LineNumber
                    }
                }
            } | Sort-Object Time -Descending | Select-Object -First 5 | ForEach-Object {
                Write-Host \"  \$(\$_.Endpoint): \$(\$_.Time)ms (Line \$(\$_.Line))\" -ForegroundColor DarkYellow
            }
        }
    } else {
        Write-Host '⚠️ No API calls logged' -ForegroundColor Yellow
    }
} else {
    Write-Host '❌ No log file found' -ForegroundColor Red
}
"
```

## Step 7: Real-time Log Monitoring

Tail the log file in real-time (like tail -f):

```bash
powershell.exe -Command "
\$today = Get-Date -Format 'yyyyMMdd'
\$logPath = \"C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\websitebuilder-\$today.txt\"

if (Test-Path \$logPath) {
    Write-Host '📡 MONITORING LOGS IN REAL-TIME' -ForegroundColor Cyan
    Write-Host 'Press Ctrl+C to stop' -ForegroundColor Gray
    Write-Host ''
    
    Get-Content \$logPath -Wait -Tail 10 | ForEach-Object {
        if (\$_ -match '\[ERR\]|Error') {
            Write-Host \$_ -ForegroundColor Red
        } elseif (\$_ -match '\[WRN\]|Warning') {
            Write-Host \$_ -ForegroundColor Yellow
        } elseif (\$_ -match '\[INF\]') {
            Write-Host \$_ -ForegroundColor Green
        } elseif (\$_ -match 'Frontend') {
            Write-Host \$_ -ForegroundColor Cyan
        } else {
            Write-Host \$_
        }
    }
} else {
    Write-Host '❌ No log file found' -ForegroundColor Red
}
"
```

## Step 8: Summary Statistics

Get a quick overview of log statistics:

```bash
powershell.exe -Command "
\$today = Get-Date -Format 'yyyyMMdd'
\$logPath = \"C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\websitebuilder-\$today.txt\"

if (Test-Path \$logPath) {
    Write-Host ''
    Write-Host '📊 LOG STATISTICS:' -ForegroundColor Cyan
    Write-Host '==================' -ForegroundColor Gray
    
    \$content = Get-Content \$logPath
    \$totalLines = \$content.Count
    \$errors = (\$content | Select-String '\[ERR\]').Count
    \$warnings = (\$content | Select-String '\[WRN\]').Count
    \$info = (\$content | Select-String '\[INF\]').Count
    \$debug = (\$content | Select-String '\[DBG\]').Count
    \$frontend = (\$content | Select-String 'Frontend').Count
    
    Write-Host \"Total log entries: \$totalLines\" -ForegroundColor White
    Write-Host ''
    Write-Host \"Errors:   \$errors\" -ForegroundColor Red
    Write-Host \"Warnings: \$warnings\" -ForegroundColor Yellow
    Write-Host \"Info:     \$info\" -ForegroundColor Green
    Write-Host \"Debug:    \$debug\" -ForegroundColor Gray
    Write-Host \"Frontend: \$frontend\" -ForegroundColor Cyan
    
    if (\$errors -gt 0) {
        Write-Host ''
        Write-Host \"⚠️ There are \$errors errors that need attention!\" -ForegroundColor Red
    } else {
        Write-Host ''
        Write-Host '✅ No errors detected today!' -ForegroundColor Green
    }
} else {
    Write-Host '❌ No log file found' -ForegroundColor Red
}
"
```

## Usage Examples

### View all logs
```
/logs
```

### Search for specific errors
```
/logs errors
```

### Monitor in real-time
```
/logs monitor
```

### Get performance stats
```
/logs performance
```

### Frontend logs only
```
/logs frontend
```

## Important Notes

1. **Backend must be running from PowerShell** to see all logs
2. **Frontend logger must be initialized** to see frontend logs
3. Logs are stored in `/logs/websitebuilder-{date}.txt`
4. Log files rotate daily
5. Frontend logs are batched and sent every 5 seconds

## Troubleshooting

If no logs are found:
1. Ensure backend is running with `dotnet run` from PowerShell
2. Check that the `/logs` directory exists
3. Verify LoggerInitializer is active in frontend
4. Confirm the date format matches (yyyyMMdd)