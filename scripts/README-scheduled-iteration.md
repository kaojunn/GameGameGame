# 定时提醒：game-design-self-iteration

## 重要说明

**Cursor 的 Skill 是写给 Agent 的说明文档，不能像后台服务一样「自动跑完」一次完整迭代。**  
本仓库提供的定时任务只会：

- 每 **30 分钟** 往 `TestAI/Documentation/IterationReports/scheduler.log` **追加一行时间戳**；
- 可选（见下）在 macOS 上 **弹系统通知**，提醒你在 Cursor 里让 Agent 执行 **`game-design-self-iteration`**。

真正的「自动迭代」（调研、改代码、写报告）仍须在 **Cursor 对话**里触发该 skill。

## macOS（launchd，推荐）

1. 赋予执行权限（首次）：

```bash
chmod +x scripts/scheduled-game-design-iteration.sh
chmod +x scripts/install-northern-town-iteration-launchd.sh
chmod +x scripts/uninstall-northern-town-iteration-launchd.sh
```

2. 安装并加载：

```bash
cd /Users/a111/Workk/ForCursor/GameDemo
./scripts/install-northern-town-iteration-launchd.sh
```

3. 卸载：

```bash
./scripts/uninstall-northern-town-iteration-launchd.sh
```

4. 可选：需要 **每半小时弹通知** 时，编辑 `~/Library/LaunchAgents/com.gamedemo.northern-town-iteration.plist`，在 `<dict>` 内增加：

```xml
<key>EnvironmentVariables</key>
<dict>
  <key>NT_ITERATION_NOTIFY</key>
  <string>1</string>
</dict>
```

然后执行：

```bash
launchctl unload ~/Library/LaunchAgents/com.gamedemo.northern-town-iteration.plist
launchctl load ~/Library/LaunchAgents/com.gamedemo.northern-town-iteration.plist
```

## 通用（cron）

若不用 launchd，可把下面一行加入 `crontab -e`（把路径改成你的仓库路径）：

```cron
*/30 * * * * /Users/a111/Workk/ForCursor/GameDemo/scripts/scheduled-game-design-iteration.sh
```

## 日志与排错

| 文件 | 说明 |
|------|------|
| `TestAI/Documentation/IterationReports/scheduler.log` | 钩子脚本写入的提醒记录 |
| `scripts/launchd-stdout.log` / `launchd-stderr.log` | launchd 捕获的标准输出/错误 |

## 若未来有 Cursor API / CLI

可将 `scheduled-game-design-iteration.sh` 中的「仅记录」替换为对官方 CLI 的调用；本脚本路径保持不变即可。
