#!/usr/bin/env bash
# 安装 macOS launchd 定时任务：每 1800 秒（30 分钟）运行一次 scheduled-game-design-iteration.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
HOOK="$SCRIPT_DIR/scheduled-game-design-iteration.sh"
LABEL="com.gamedemo.northern-town-iteration"
PLIST_DST="$HOME/Library/LaunchAgents/${LABEL}.plist"

if [[ ! -x "$HOOK" ]]; then
  chmod +x "$HOOK"
fi

cat >"$PLIST_DST" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>${LABEL}</string>
  <key>ProgramArguments</key>
  <array>
    <string>/bin/bash</string>
    <string>${HOOK}</string>
  </array>
  <key>StartInterval</key>
  <integer>1800</integer>
  <key>RunAtLoad</key>
  <false/>
  <key>StandardOutPath</key>
  <string>${SCRIPT_DIR}/launchd-stdout.log</string>
  <key>StandardErrorPath</key>
  <string>${SCRIPT_DIR}/launchd-stderr.log</string>
</dict>
</plist>
EOF

if launchctl list | grep -q "$LABEL"; then
  launchctl unload "$PLIST_DST" 2>/dev/null || true
fi
launchctl load "$PLIST_DST"

echo "已安装并加载: $PLIST_DST"
echo "间隔: 1800 秒（30 分钟）。日志: TestAI/Documentation/IterationReports/scheduler.log"
echo "卸载: ./uninstall-northern-town-iteration-launchd.sh"
