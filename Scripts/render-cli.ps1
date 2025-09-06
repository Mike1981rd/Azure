$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Resolve-Path (Join-Path $ScriptDir '..')

# Load token
if (-not $env:RENDER_API_KEY) {
  $envFile = Join-Path (Join-Path $RootDir 'websitebuilder-admin') '.secure/render.env'
  if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
      if ($_ -match '^[A-Za-z_][A-Za-z0-9_]*=') {
        $parts = $_.Split('=',2)
        if ($parts.Length -eq 2) { [System.Environment]::SetEnvironmentVariable($parts[0], $parts[1]) }
      }
    }
  } elseif (Test-Path "$HOME/.render-token") {
    Get-Content "$HOME/.render-token" | ForEach-Object {
      if ($_ -match '^[A-Za-z_][A-Za-z0-9_]*=') {
        $parts = $_.Split('=',2)
        if ($parts.Length -eq 2) { [System.Environment]::SetEnvironmentVariable($parts[0], $parts[1]) }
      }
    }
  }
}

if (-not $env:RENDER_API_KEY) {
  Write-Warning "[render-cli] No RENDER_API_KEY set."
}

# Prefer local render binary
if (Get-Command render -ErrorAction SilentlyContinue) {
  render @args
} elseif (Test-Path (Join-Path $RootDir 'render.exe')) {
  & (Join-Path $RootDir 'render.exe') @args
} elseif (Test-Path (Join-Path $RootDir 'render')) {
  & (Join-Path $RootDir 'render') @args
} else {
  Write-Error "[render-cli] Render CLI not found in PATH nor repo root."
  exit 127
}

