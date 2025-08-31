#!/usr/bin/env bash

# Deploy script for WebsiteBuilder API to Azure VM
# - Builds locally and syncs publish output to the VM
# - Restarts the systemd service (or falls back to screen wrapper)
#
# Prereqs on your machine:
# - .NET SDK 8+ (dotnet)
# - ssh/scp/rsync installed

set -euo pipefail

HOST="azureuser@20.169.209.166"
REMOTE_DIR="/home/azureuser/websitebuilder-app"
PUBLISH_DIR="publish"

echo "[1/4] Building release (dotnet publish)"
dotnet publish -c Release -o "$PUBLISH_DIR"

echo "[2/4] Syncing files to VM ($HOST:$REMOTE_DIR)"
if command -v rsync >/dev/null 2>&1; then
  rsync -avz --delete "$PUBLISH_DIR"/ "$HOST:$REMOTE_DIR/"
else
  echo "rsync not found; falling back to scp (no delete)."
  scp -r "$PUBLISH_DIR"/* "$HOST:$REMOTE_DIR/"
fi

echo "[3/4] Restarting app service on VM"
ssh "$HOST" "sudo systemctl daemon-reload || true; sudo systemctl restart websitebuilder || (screen -S api -X quit || true; sleep 1; screen -dmS api $REMOTE_DIR/run-api.sh)"

echo "[4/4] Health check via Nginx proxy"
ssh "$HOST" "curl -sS -o /dev/null -w 'HTTP:%{http_code}\n' http://localhost/health"

echo "Done."

