#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Load token
if [ -z "${RENDER_API_KEY:-}" ]; then
  if [ -f "$ROOT_DIR/websitebuilder-admin/.secure/render.env" ]; then
    # shellcheck disable=SC2046
    export $(grep -E '^[A-Za-z_][A-Za-z0-9_]*=' "$ROOT_DIR/websitebuilder-admin/.secure/render.env" | xargs)
  elif [ -f "$HOME/.render-token" ]; then
    # shellcheck disable=SC2046
    export $(grep -E '^[A-Za-z_][A-Za-z0-9_]*=' "$HOME/.render-token" | xargs)
  fi
fi

if [ -z "${RENDER_API_KEY:-}" ]; then
  echo "[render-cli] No RENDER_API_KEY set."
fi

# Prefer local render binary if present
if command -v render >/dev/null 2>&1; then
  exec render "$@"
elif [ -x "$ROOT_DIR/render" ]; then
  exec "$ROOT_DIR/render" "$@"
elif [ -x "$ROOT_DIR/render.exe" ]; then
  exec "$ROOT_DIR/render.exe" "$@"
else
  echo "[render-cli] Render CLI not found in PATH nor repo root."
  exit 127
fi

