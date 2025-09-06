$ErrorActionPreference = 'Stop'

if (-not $env:NEON_API_KEY) {
  $repoEnv = Join-Path (Join-Path $PSScriptRoot '..') 'websitebuilder-admin/.secure/neon.env'
  if (Test-Path $repoEnv) {
    Get-Content $repoEnv | ForEach-Object {
      if ($_ -match '^[A-Za-z_][A-Za-z0-9_]*=') {
        $parts = $_.Split('=',2)
        if ($parts.Length -eq 2) { [System.Environment]::SetEnvironmentVariable($parts[0], $parts[1]) }
      }
    }
  } elseif (Test-Path (Join-Path $HOME '.neon-token')) {
    Get-Content (Join-Path $HOME '.neon-token') | ForEach-Object {
      if ($_ -match '^[A-Za-z_][A-Za-z0-9_]*=') {
        $parts = $_.Split('=',2)
        if ($parts.Length -eq 2) { [System.Environment]::SetEnvironmentVariable($parts[0], $parts[1]) }
      }
    }
  }
}

if (-not (Get-Command neonctl -ErrorAction SilentlyContinue)) {
  Write-Error "[neon-cli] 'neonctl' not found in PATH. Install Neon CLI or use API."
  exit 127
}

neonctl @args
