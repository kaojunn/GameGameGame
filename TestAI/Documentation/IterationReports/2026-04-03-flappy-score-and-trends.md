# 迭代报告：Flappy 分数与高分持久化（2026-04-03）

## 1. 迭代前玩法摘要

- **操作**：鼠标左键 / 空格向上冲量；小鸟世界 X 固定，Y 受重力与跳跃影响。
- **障碍**：运行时生成上下绿色立方体柱，整体左移，缺口高度随机。
- **胜负**：碰撞柱体或落入 DeathZone 即死亡；死后禁用输入与刚体；R 重载场景。
- **反馈**：Kenney CC0 音频（BGM、拍翼、撞击）；OnGUI 操作与结束提示。
- **缺失**：无穿缝计分、无历史最佳、弱重玩动机。

## 2. 外部趋势归纳（对照用）

以下为公开讨论中常见的独立游戏范式标签，用于对比而非精确市场数据：

1. **Roguelike 牌组构筑**：单局内从随机奖励构筑牌组（如 *Slay the Spire* 系）。与本原型差距大，短期不适合作为小迭代目标。
2. **Bullet Heaven / 类幸存者**：自动输出、密集敌人、局内升级链（如 *Vampire Survivors* 系；Steam「Bullet Heaven」标签趋势）。完整移植成本高，可借鉴「高频反馈、数字刺激」。
3. **短局 Roguelite + 分数或 Meta**：单局短、死亡常见，靠高分、解锁、局外成长维持重玩（街机式高分与部分 roguelite 共通）。与本项目最接近、改造成本最低。

## 3. 对比结论

| 范式 | 与本项目关系 | 本次是否采纳 |
|------|----------------|--------------|
| 牌组构筑 | 需重做核心循环 | 否 |
| Bullet Heaven | 可借鉴反馈密度 | 部分（得分音效 + HUD 数字） |
| 分数 / Meta | 直接可加分与持久化 | **是**（PlayerPrefs 最高分） |

## 4. 本次实现清单

- **`PipeSpawner`**：每对柱子之间生成薄盒体触发器 + **`PipeScoreGate`**（一次性计分）。
- **`FlappyScoreManager`**：当前分、死亡冻结、`LastRunScore`、`BestScore`、`WasNewRecord`，键名 `FlappyBirdBestScore`。
- **`FlappyDeath.Die`**：优先调用 `FreezeOnDeath()` 再处理物理与 UI。
- **`FlappyAudio`**：`PlayScorePing()`（复用 flap 音频、较低音量）。
- **`FlappyHud`**：运行时创建 Screen Space Overlay Canvas + **Unity UI Text**（系统字体），左上角显示分数/最佳。
- **`FlappyUI`**：游戏结束面板增加本局分数、最佳、新纪录提示。
- **场景**：`FlappySystems` 挂载 `FlappyScoreManager`、`FlappyHud`；`FlappyAudio` 增加 `scorePingVolume` 序列化字段。

## 5. 后续可选方向（未做）

- 难度随分数提高（缩短生成间隔等）。
- 粒子或简单屏幕闪反馈得分。
- 牌组或类幸存者机制（需更大范围重构）。

## 6. 参考链接（趋势语境）

- PC Gamer 等对 roguelike deckbuilder 品类的报道。
- Multiplier / GamingOnLinux 等对「Bullet Heaven」品类命名与趋势的讨论。

（具体 URL 可按需自行补充到版本控制外备注。）
