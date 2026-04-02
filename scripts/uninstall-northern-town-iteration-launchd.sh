#!/usr/bin/env bash
set -euo pipefail

LABEL="com.gamedemo.northern-town-iteration"
PLIST_DST="$HOME/Library/LaunchAgents/${LABEL}.plist"

if [[ -f "$PLIST_DST" ]]; then
  launchctl unload "$PLIST_DST" 2>/dev/null || true
  rm -f "$PLIST_DST"
  echo "已卸载: $PLIST_DST"
else
  echo "未找到 plist: $PLIST_DST"
fi
