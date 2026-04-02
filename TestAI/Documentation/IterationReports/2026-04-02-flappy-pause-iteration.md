# 迭代报告：Flappy 暂停功能小迭代（2026-04-02）

## 1. 迭代前玩法（现状还原）

- **输入操作**：鼠标左键或空格让小鸟向上冲量（`BirdFlap`），小鸟 X 坐标固定，Y 受重力和冲量影响。
- **目标与进度**：穿过管道中缝触发计分门（`PipeScoreGate`），`FlappyScoreManager` 统计本局分与最高分（`PlayerPrefs` 键名：`FlappyBirdBestScore`）。
- **失败/结束**：碰到绿色柱体或进入 `DeathZone` 触发死亡（`FlappyDeath`），冻结分数、禁用移动输入，展示结束 UI。
- **反馈**：左上 HUD 显示当前分/最佳分（`FlappyHud`）；死亡弹框支持 R 重开（`FlappyUI`）；音效由 `FlappyAudio` 播放（拍翼、得分、死亡）。
- **场景挂载确认**：`Assets/Scenes/SampleScene.unity` 中 `FlappySystems` 已挂 `PipeSpawner`、`FlappyUI`、`FlappyAudio`、`FlappyScoreManager`、`FlappyHud`，闭环完整。

## 2. 趋势要点（外部检索浓缩）

1. **可访问性语境：实时玩法建议支持暂停/缓冲操作**  
   Game Accessibility Guidelines 提到，不应让精确时机成为唯一完成路径，可提供暂停期间可执行的替代方式，降低高反应门槛。  
   来源语境：Game Accessibility Guidelines（完整列表及相关条目）。  
   - https://gameaccessibilityguidelines.com/full-list/  
   - https://gameaccessibilityguidelines.com/do-not-make-precise-timing-essential-to-gameplay-offer-alternatives-actions-that-can-be-carried-out-while-paused-or-a-skip-mechanism/

2. **平台可访问性语境：暂停菜单属于常见交互入口**  
   Xbox Accessibility Guidelines 在 UI 一致性中强调可预测导航与一致交互，暂停菜单是常见承载场景。  
   来源语境：Microsoft Learn（Xbox Accessibility Guidelines）。  
   - https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/112  
   - https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/

3. **短局休闲语境：玩家会被现实打断，恢复成本应低**  
   Hyper-casual 讨论普遍围绕“短时可玩、随时中断再继续”，虽然具体实现各异，但“可恢复性”是高频诉求。  
   来源语境：行业文章/综述型资料（用于方向参照，而非学术统计）。  
   - https://www.adjust.com/blog/how-to-make-a-hyper-casual-game-successful/  
   - https://en.wikipedia.org/wiki/Hypercasual_game

## 3. 对比结论（只选一项）

| 趋势/范式 | 与当前项目差距 | 适不适合本次小迭代 |
|---|---|---|
| 复杂可访问性体系（重映射、色弱完整方案） | 涉及输入层与大量 UI 资源，改动面大 | 否 |
| 难度动态调节（速度/间距随分数变化） | 需重新平衡玩法节奏，有回归风险 | 否 |
| **即时暂停/继续（P/Esc）** | 仅需少量脚本逻辑与 UI 提示 | **是（本次实现）** |

**选择理由**：改动面小、依赖少、验收标准清晰（可暂停/可恢复/不破坏死亡与重开流程）。

## 4. 本次改动列表

- `Assets/FlappyBird/Scripts/FlappyUI.cs`
  - 新增 `_paused` 状态。
  - 新增 `TogglePause()` / `SetPaused(bool)`。
  - 新增静态查询 `IsGameplayPaused()` 供其他脚本读取。
  - `OnGUI` 增加暂停提示与“继续游戏”按钮。
  - 顶部操作提示更新为“P / Esc = 暂停”。
  - `ShowGameOver()` 时强制解除暂停，避免状态冲突。
  - `OnDisable()` 中兜底恢复 `Time.timeScale = 1`。

- `Assets/FlappyBird/Scripts/BirdFlap.cs`
  - 在输入处理前增加 `FlappyUI.IsGameplayPaused()` 判断，暂停时不响应跳跃输入。

- `Assets/FlappyBird/Scripts/FlappyDeath.cs`
  - `ReloadScene()` 前显式 `Time.timeScale = 1f`，避免暂停态跨场景残留。

## 5. 未做后续项（刻意留待以后）

- 暂停时增加“返回主菜单/设置”多按钮菜单。
- 暂停时降低/静音 BGM（当前仅冻结时间，不单独混音处理）。
- 输入重映射与手柄专用提示。
- 更完整的可访问性方案（视觉对比度、字幕、色彩方案等）。

## 6. 验收自检（逻辑推演）

- **暂停行为**：局内按 `P` 或 `Esc` 后，`Time.timeScale` 置 0，左上继续显示 HUD，中央出现“游戏已暂停”弹框与继续按钮。
- **恢复行为**：再次按 `P/Esc` 或点击继续按钮，`Time.timeScale` 置 1，障碍移动与输入恢复。
- **死亡回归**：死亡时 `ShowGameOver()` 会解除暂停，结束面板可正常显示并按 `R` 重开。
- **重开回归**：`ReloadScene()` 先将 `Time.timeScale` 归 1，避免新局卡在暂停态。
- **持久化回归**：本次未新增 `PlayerPrefs` 键，仍使用既有 `FlappyBirdBestScore`，无键名冲突。
