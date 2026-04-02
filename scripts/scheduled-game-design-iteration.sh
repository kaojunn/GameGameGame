#!/usr/bin/env bash
# 定时钩子：无法在无 Cursor 会话时真正「执行」AI skill；本脚本负责记录时间、
# 可选系统通知，提醒在本仓库中手动/让 Agent 执行 game-design-self-iteration。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
LOG_DIR="$REPO_ROOT/TestAI/Documentation/IterationReports"
mkdir -p "$LOG_DIR"
LOG_FILE="$LOG_DIR/scheduler.log"
TS="$(date '+%Y-%m-%dT%H:%M:%S%z')"

{
  echo "[$TS] tick — 请在 Cursor 中对本仓库执行 skill：game-design-self-iteration（游戏设计自我迭代）"
} >>"$LOG_FILE"

# 可选：每半小时弹一次通知（可能打扰；默认关闭）
# 启用：launchctl setenv NT_ITERATION_NOTIFY 1 或在 plist 里设置 EnvironmentVariables
if [[ "${NT_ITERATION_NOTIFY:-0}" == "1" ]] && command -v osascript >/dev/null 2>&1; then
  osascript -e 'display notification "在 Cursor 中执行 game-design-self-iteration（北方小镇）" with title "GameDemo 定时迭代"' 2>/dev/null || true
fi

exit 0
