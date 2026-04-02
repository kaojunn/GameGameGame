using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NorthernTown2026
{
    /// <summary>玩家属性、等级、经验、装备。</summary>
    [Serializable]
    public class PlayerState
    {
        const string EndingPrefKeyPrefix = "NorthernTown2026.EndingUnlocked.";

        public int Level = 1;
        public int CurrentXp;
        public int 体魄 = 3;
        public int 洞察 = 3;
        public int 镇定 = 3;
        public int 机巧 = 3;

        public readonly HashSet<string> InventoryItemIds = new HashSet<string>();
        public readonly Dictionary<string, string> EquippedBySlot = new Dictionary<string, string>();
        public readonly HashSet<string> UnlockedEndingIds = new HashSet<string>();
        bool _endingMetaLoaded;

        /// <summary>本局已展示的剧情节点次数（含起始节点；周目重置时清零）。</summary>
        public int RunNodesVisitedCount;

        /// <summary>本局已执行的选项次数（周目重置时清零）。</summary>
        public int RunChoicesCount;

        static readonly int[] XpToNextLevel = { 0, 80, 160, 260, 380, 520 };

        public int GetStat(StatId id)
        {
            int v = id switch
            {
                StatId.体魄 => 体魄,
                StatId.洞察 => 洞察,
                StatId.镇定 => 镇定,
                StatId.机巧 => 机巧,
                _ => 0
            };
            foreach (var kv in EquippedBySlot)
            {
                if (!EquipmentCatalog.TryGet(kv.Value, out var def))
                    continue;
                v += id switch
                {
                    StatId.体魄 => def.Bonus体魄,
                    StatId.洞察 => def.Bonus洞察,
                    StatId.镇定 => def.Bonus镇定,
                    StatId.机巧 => def.Bonus机巧,
                    _ => 0
                };
            }
            return v;
        }

        public int XpRequiredForNextLevel()
        {
            if (Level >= XpToNextLevel.Length)
                return 9999;
            return XpToNextLevel[Level];
        }

        public bool GrantXp(int amount, out string message)
        {
            message = null;
            if (amount <= 0)
                return false;
            CurrentXp += amount;
            var need = XpRequiredForNextLevel();
            if (Level < XpToNextLevel.Length && CurrentXp >= need)
            {
                CurrentXp -= need;
                Level++;
                体魄++;
                洞察++;
                镇定++;
                机巧++;
                message = $"【升级】到达 Lv.{Level}，四项属性各 +1。";
                return true;
            }
            return false;
        }

        public bool TryEquip(string itemId, out string msg) => TryEquipToSlot(itemId, null, out msg);

        /// <summary>拖入槽位时调用；slot 为 null 时使用物品定义中的默认槽位。</summary>
        public bool TryEquipToSlot(string itemId, string slot, out string msg)
        {
            msg = null;
            if (!EquipmentCatalog.TryGet(itemId, out var def))
            {
                msg = "未知物品。";
                return false;
            }
            if (!InventoryItemIds.Contains(itemId))
            {
                msg = "你身上没有这件物品。";
                return false;
            }
            if (def.Consumable)
            {
                msg = "消耗品请拖入「使用」区域。";
                return false;
            }
            var targetSlot = string.IsNullOrEmpty(slot) ? def.Slot : slot;
            if (def.Slot != targetSlot)
            {
                msg = $"该物品只能放入「{def.Slot}」槽位。";
                return false;
            }
            var removeKeys = new List<string>();
            foreach (var kv in EquippedBySlot)
            {
                if (kv.Value == itemId)
                    removeKeys.Add(kv.Key);
            }
            foreach (var k in removeKeys)
                EquippedBySlot.Remove(k);
            EquippedBySlot[targetSlot] = itemId;
            msg = $"已装备：{def.Name}（{targetSlot}）";
            return true;
        }

        public bool TryUnequipSlot(string slot, out string msg)
        {
            msg = null;
            if (!EquippedBySlot.TryGetValue(slot, out var itemId))
            {
                msg = "该槽位为空。";
                return false;
            }
            EquippedBySlot.Remove(slot);
            msg = $"已卸下：{(EquipmentCatalog.TryGet(itemId, out var d) ? d.Name : itemId)}";
            return true;
        }

        public void AddItem(string itemId)
        {
            if (!string.IsNullOrEmpty(itemId))
                InventoryItemIds.Add(itemId);
        }

        public bool HasItem(string itemId) => !string.IsNullOrEmpty(itemId) && InventoryItemIds.Contains(itemId);

        void EnsureEndingMetaLoaded()
        {
            if (_endingMetaLoaded)
                return;
            _endingMetaLoaded = true;
            UnlockedEndingIds.Clear();
            foreach (var endingId in NorthernTownStoryContent.EndingNodeIds)
            {
                if (PlayerPrefs.GetInt(EndingPrefKeyPrefix + endingId, 0) == 1)
                    UnlockedEndingIds.Add(endingId);
            }
        }

        /// <summary>收录结局（跨周目持久化）。返回 false 表示不是有效结局 ID。</summary>
        public bool MarkEndingUnlocked(string endingNodeId, out bool isNewUnlock)
        {
            EnsureEndingMetaLoaded();
            isNewUnlock = false;
            if (!NorthernTownStoryContent.IsEndingNode(endingNodeId))
                return false;
            if (!UnlockedEndingIds.Add(endingNodeId))
                return true;
            isNewUnlock = true;
            PlayerPrefs.SetInt(EndingPrefKeyPrefix + endingNodeId, 1);
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>消耗背包中的消耗品并结算经验（含升级）。</summary>
        public bool TryConsumeItem(string itemId, out string msg)
        {
            msg = null;
            if (!EquipmentCatalog.TryGet(itemId, out var def))
            {
                msg = "未知物品。";
                return false;
            }
            if (!def.Consumable)
            {
                msg = "该物品无法直接使用。";
                return false;
            }
            if (!InventoryItemIds.Contains(itemId))
            {
                msg = "你身上没有这件物品。";
                return false;
            }
            var removeKeys = new List<string>();
            foreach (var kv in EquippedBySlot)
            {
                if (kv.Value == itemId)
                    removeKeys.Add(kv.Key);
            }
            foreach (var k in removeKeys)
                EquippedBySlot.Remove(k);
            InventoryItemIds.Remove(itemId);

            var xp = def.GrantXpOnUse;
            GrantXp(xp, out var lvlMsg);
            var sb = new StringBuilder();
            sb.Append($"使用了：{def.Name}。");
            if (xp > 0)
                sb.Append($"获得经验 +{xp}。");
            if (!string.IsNullOrEmpty(lvlMsg))
                sb.Append(lvlMsg);
            msg = sb.ToString();
            return true;
        }

        public void ResetForNewGame()
        {
            EnsureEndingMetaLoaded();
            Level = 1;
            CurrentXp = 0;
            体魄 = 洞察 = 镇定 = 机巧 = 3;
            InventoryItemIds.Clear();
            EquippedBySlot.Clear();
            RunNodesVisitedCount = 0;
            RunChoicesCount = 0;
            AddItem("item_bread");
        }

        public string FormatStatusBlock()
        {
            EnsureEndingMetaLoaded();
            var sb = new StringBuilder();
            sb.AppendLine($"Lv.{Level}  经验 {CurrentXp}/{XpRequiredForNextLevel()}");
            sb.AppendLine($"本局进度：已读节点 {RunNodesVisitedCount} · 已做选择 {RunChoicesCount}");
            sb.AppendLine($"结局图鉴：{UnlockedEndingIds.Count}/{NorthernTownStoryContent.EndingNodeIds.Count}");
            if (UnlockedEndingIds.Count > 0)
            {
                sb.Append("已收录：");
                var first = true;
                foreach (var endingId in NorthernTownStoryContent.EndingNodeIds)
                {
                    if (!UnlockedEndingIds.Contains(endingId))
                        continue;
                    if (!first)
                        sb.Append('、');
                    sb.Append(NorthernTownStoryContent.GetEndingDisplayName(endingId));
                    first = false;
                }
                sb.AppendLine();
            }
            sb.AppendLine($"体魄 {GetStat(StatId.体魄)}  洞察 {GetStat(StatId.洞察)}  镇定 {GetStat(StatId.镇定)}  机巧 {GetStat(StatId.机巧)}");
            sb.AppendLine("— 装备 —");
            foreach (var slot in new[] { "终端", "外套", "饰品" })
            {
                if (EquippedBySlot.TryGetValue(slot, out var id) && EquipmentCatalog.TryGet(id, out var d))
                    sb.AppendLine($"{slot}：{d.Name}");
                else
                    sb.AppendLine($"{slot}：（空）");
            }
            sb.AppendLine("— 背包（未装备）—");
            var inSlot = new HashSet<string>(EquippedBySlot.Values);
            var any = false;
            foreach (var id in InventoryItemIds)
            {
                if (EquipmentCatalog.TryGet(id, out var d))
                {
                    if (inSlot.Contains(id))
                        continue;
                    sb.AppendLine($"· {d.Name}");
                    any = true;
                }
                else
                {
                    sb.AppendLine($"· {id}");
                    any = true;
                }
            }
            if (!any)
                sb.AppendLine("（无）");
            return sb.ToString();
        }
    }
}
