# 玩法框架说明（无剧情）

本文档描述「北方小镇 2026」当前**可迭代的玩法与系统边界**，便于在不动剧情文本的前提下扩展规则。剧情节点、对白与具体分支见内容数据，不在此展开。

---

## 1. 核心循环

- **图结构**：由若干 `StoryNode`（`Id`、`Text`、`Choices`）组成的有向图；引擎维护 `CurrentNodeId`，展示当前节点文本与可用选项。
- **推进方式**：玩家从当前节点的 `ChoiceOption` 中选择一项，引擎按规则结算后跳转到下一节点或结束。
- **日志**：节点文本与系统消息（检定、获得物品/经验、装备变更等）写入叙事区，状态面板与选项按钮随 `OnStateChanged` 刷新。

**入口**：`TextAdventureEngine.Start(startNodeId)` 会先执行 `PlayerState.ResetForNewGame()`，再进入起始节点（默认 `"start"`）。

---

## 2. 玩家状态（`PlayerState`）

| 概念 | 说明 |
|------|------|
| 等级 `Level` | 从 1 起算；升级时四项基础属性各 +1。 |
| 当前经验 `CurrentXp` | 累计值；达到本级所需后扣减并升级（见下表）。 |
| 基础四维 | `体魄`、`洞察`、`镇定`、`机巧`，初始均为 3。 |
| **有效属性** | `GetStat(StatId)` = 基础值 + **已装备物品**在目录中的四维加成之和。 |
| 背包 | `InventoryItemIds`：字符串 ID 集合（**无堆叠数量**，同一 ID 至多出现一次语义上由集合保证）。 |
| 装备 | `EquippedBySlot`：槽位名 → 物品 ID；物品仍在背包集合中，界面「未装备」列表会排除已占用同一物品的槽位展示。 |
| 本局进度 | `RunNodesVisitedCount`：每次成功展示并记录一个剧情节点 +1；`RunChoicesCount`：每次执行一次选项 +1；`ResetForNewGame` 时清零。状态面板文本展示一行「已读节点 / 已做选择」。 |
| 结局图鉴（Meta） | 引擎启动时扫描 `ending_` 前缀节点，`PlayerState` 从 `PlayerPrefs` 读取已解锁结局并在状态面板显示「结局图鉴：x/y（跨周目）」。首次进入某个结局节点会写入键 `NorthernTown2026.EndingUnlocked.<nodeId>`。 |

**升级所需经验**（`XpToNextLevel`，下标为当前 `Level`）：  
`80, 160, 260, 380, 520`（数组长度之外视为需 9999，用于封顶表现）。

**新游戏 / 周目重置**：`ResetForNewGame()` 清空等级、经验、四维（回到初始）、背包与装备，并**发放开局消耗品**（当前：`item_bread`）。剧情中若选项将 `NextNodeId` 设为 `"start"`，引擎会再次调用 `ResetForNewGame()` 并触发 `OnNewRunStarted`。

---

## 3. 物品与目录（`EquipmentCatalog` / `EquipmentDefinition`）

- 所有在目录中注册的物品有统一结构：`Id`、`Name`、`Slot`、四维加成、`Consumable`、`GrantXpOnUse`。
- **装备**：`Consumable == false`，且 `Slot` 为 **`终端` / `外套` / `饰品`** 之一（与 UI 槽位一致）。装备时**目标槽位必须等于**物品定义的 `Slot`。
- **消耗品**：`Consumable == true`，**不可**通过装备槽穿戴；仅能通过 UI「使用」区消耗，消耗时按 `GrantXpOnUse` 调用 `GrantXp`。
- **非目录物品**：仍可存在于 `InventoryItemIds`（例如剧情发放的原始 ID）；不参与装备数值，在背包中以 ID 字符串展示。

扩展玩法时：在 `EquipmentCatalog` 静态构造中 `Register` 新定义即可；若新增槽位类型，需同步 **UI 槽位名**、`PlayerState.FormatStatusBlock` 中的槽位列表与 `TextAdventureBootstrap` 里生成的槽位。

---

## 4. 选项与结算（`ChoiceOption` / `Choose`）

每条选项可组合以下效果（按实现顺序）：

1. **发放物品**：`GrantItemId` → `AddItem`（无则不加）。
2. **发放经验**：`GrantXp` → `GrantXp`；若升级，先记录升级提示再记录「获得经验 +n」类日志。
3. **检定**（若 `Check != null`）：  
   - 总值 = `GetStat(检定的 StatId)` + **1～10 随机整数**；  
   - 与 `Threshold` 比较，成功走 `SuccessNodeId`，失败走 `FailNodeId`。
4. **无检定**：走 `NextNodeId`。
5. **结束**：`NextNodeId` 为空时记「剧终」，不跳转。
6. **跳转 `start`**：在更新 `CurrentNodeId` 之前执行 `ResetForNewGame()`（新周目）。

**选项可见性**（过滤，不满足则不显示）：

- `RequiresItemId`：玩家背包需包含该 ID。
- `RequiresInsightSum`：`洞察 + 机巧` 的有效值之和需 ≥ 该值。

---

## 5. UI 与拖放（运行时生成）

右侧面板大致自上而下：**状态文本 → 装备槽（三格）→ 使用区（消耗品）→ 背包网格**。独立 **DragLayer** 用于拖动中置顶显示。

**拖放判定顺序**（`HandleEquipmentCardDrop`）：

1. **使用区**：命中则消耗品走 `ApplyConsumeFromDrag`；非消耗品提示「该物品无法在此使用。」并刷新。
2. **装备槽**：`ApplyEquipFromDrag`（槽位 key 与物品 `Slot` 一致才成功）。
3. **背包区**：若卡片来自槽位则 `ApplyUnequipFromDrag`；否则仅刷新（归位）。
4. **其它区域**：`RefreshUiOnly`，卡片按数据重建，表现为归位。

**技术要点**：拖动时卡片 `CanvasGroup.blocksRaycasts = false`，便于射线命中槽位与区域；槽位与区域可用射线 + 屏幕矩形兜底检测。

---

## 6. 与剧情内容的边界

- **玩法框架**：引擎、玩家状态、目录、选项字段、检定公式、UI 交互 —— 以本文档与 `Scripts` 下代码为准。
- **剧情内容**：节点文本、选项文案、图结构 —— 由故事数据/内容构建器提供，迭代时可单独替换而不改变上述规则（除非刻意新增字段或改 `ChoiceOption` 语义）。

---

## 7. 持久化键约定

- 当前玩法仅使用以下持久化键前缀：`NorthernTown2026.EndingUnlocked.`
- 该前缀专用于「结局图鉴」解锁态，避免与其他系统（如音量、视频设置、未来成就）冲突。

---

## 8. 迭代时可优先扩展的方向（建议）

- 目录：新装备槽位、堆叠、耐久、多效果消耗品。
- 选项：新条件类型、资源消耗、多段检定。
- 战斗或地图：在现有「节点 + 选项」之上再挂子系统，保持 `PlayerState` 为单一数据源。

文档版本与工程内实现一致时可随大版本更新本页日期与章节。
