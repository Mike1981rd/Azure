#!/usr/bin/env bash
set -euo pipefail

# Load token
if [ -z "${NEON_API_KEY:-}" ]; then
  if [ -f "websitebuilder-admin/.secure/neon.env" ]; then
    # shellcheck disable=SC2046
    export $(grep -E '^[A-Za-z_][A-Za-z0-9_]*=' "websitebuilder-admin/.secure/neon.env" | xargs)
  elif [ -f "$HOME/.neon-token" ]; then
    # shellcheck disable=SC2046
    export $(grep -E '^[A-Za-z_][A-Za-z0-9_]*=' "$HOME/.neon-token" | xargs)
  fi
fi

if ! command -v neonctl >/dev/null 2>&1; then
  echo "[neon-cli] 'neonctl' not found in PATH. Install Neon CLI or use API." >&2
  exit 127
fi

exec neonctl "$@"
