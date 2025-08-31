# Backend Management Command

Stop, build, and run the ASP.NET Core backend via PowerShell: $ARGUMENTS

## Step 1: Stop Current Backend Process

First, stop any running dotnet processes and free up port 5266:

```bash
# Kill all dotnet processes
powershell.exe -Command "Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force"

# Wait a moment for processes to terminate
sleep 2

# Check if port 5266 is still in use and kill that specific process
powershell.exe -Command "
\$connection = Get-NetTCPConnection -LocalPort 5266 -ErrorAction SilentlyContinue
if (\$connection) {
    Stop-Process -Id \$connection.OwningProcess -Force
    Write-Host 'Killed process using port 5266'
} else {
    Write-Host 'Port 5266 is free'
}"
```

## Step 2: Clean Build Directory

Clean the build output to ensure fresh compilation:

```bash
powershell.exe -Command "
cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI'
if (Test-Path 'bin') { Remove-Item -Path 'bin' -Recurse -Force }
if (Test-Path 'obj') { Remove-Item -Path 'obj' -Recurse -Force }
Write-Host 'Build directories cleaned'"
```

## Step 3: Restore NuGet Packages

Restore all dependencies:

```bash
powershell.exe -Command "
cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI'
dotnet restore
"
```

## Step 4: Build the Backend

Compile the backend and capture any errors:

```bash
powershell.exe -Command "
cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI'
\$output = dotnet build 2>&1
\$buildSuccess = \$LASTEXITCODE -eq 0

Write-Host '===================='
Write-Host 'BUILD OUTPUT:'
Write-Host '===================='
Write-Host \$output

if (\$buildSuccess) {
    Write-Host ''
    Write-Host '✅ BUILD SUCCESSFUL' -ForegroundColor Green
} else {
    Write-Host ''
    Write-Host '❌ BUILD FAILED' -ForegroundColor Red
    Write-Host 'Errors found in build output above'
    exit 1
}"
```

## Step 5: Check for Common Compilation Errors

If build fails, check for common issues:

1. **Namespace conflicts**: Check for duplicate class names
2. **Missing references**: Ensure all NuGet packages are restored
3. **Type mismatches**: Verify property types match across models
4. **Missing using statements**: Add required namespaces

Common fixes:
- MenuId type mismatch: Ensure it's `int?` in HeaderConfig.cs
- Missing NavigationMenu references: Add `using WebsiteBuilderAPI.Models;`
- JSON serialization issues: Ensure proper JsonIgnore attributes

## Step 6: Run the Backend

If build is successful, run the backend:

```bash
powershell.exe -Command "
cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI'
Write-Host ''
Write-Host '🚀 STARTING BACKEND...' -ForegroundColor Cyan
Write-Host 'Backend will run at: http://localhost:5266' -ForegroundColor Yellow
Write-Host 'Press Ctrl+C to stop' -ForegroundColor Gray
Write-Host ''
dotnet run --no-build
"
```

## Step 7: Verify Backend is Running

After starting, the backend should show:
- Serilog initialization messages
- Database connection established
- HTTP server listening on port 5266
- Swagger available at http://localhost:5266/swagger

## Common Issues and Solutions

### Port Already in Use
```powershell
# Force kill process on port 5266
Get-Process -Id (Get-NetTCPConnection -LocalPort 5266).OwningProcess | Stop-Process -Force
```

### Build Errors with MenuId
```csharp
// In HeaderConfig.cs, ensure:
public int? MenuId { get; set; } = null;
```

### Database Connection Failed
```powershell
# Check PostgreSQL is running
Get-Service -Name "postgresql*"
```

### Missing Migrations
```powershell
# Apply pending migrations
dotnet ef database update
```

## Output Format

The command will show:
1. ✅ Process termination status
2. 📦 Package restoration status  
3. 🔨 Build output with errors/warnings
4. 🚀 Server startup messages
5. 📊 Real-time logs from Serilog

## Notes

- This command MUST be run from Windows (PowerShell)
- Do NOT run from WSL/Linux
- Backend will run in the foreground showing logs
- Press Ctrl+C to stop the backend
- Logs are saved to `/logs/websitebuilder-{date}.txt`