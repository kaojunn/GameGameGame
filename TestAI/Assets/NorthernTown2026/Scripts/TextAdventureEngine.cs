using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NorthernTown2026
{
    public class TextAdventureEngine
    {
        readonly Dictionary<string, StoryNode> _nodes;
        readonly PlayerState _player = new PlayerState();
        readonly System.Random _rng;

        public string CurrentNodeId { get; private set; }
        public PlayerState Player => _player;

        public event Action<string> OnLog;
        public event Action OnStateChanged;
        public event Action OnNewRunStarted;

        public TextAdventureEngine(Dictionary<string, StoryNode> nodes, int? seed = null)
        {
            _nodes = nodes;
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        public void Start(string startNodeId = "start")
        {
            _player.ResetForNewGame();
            CurrentNodeId = startNodeId;
            EmitNode(startNodeId);
        }

        void EmitNode(string nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node))
            {
                OnLog?.Invoke($"【错误】找不到节点：{nodeId}");
                return;
            }
            _player.RunNodesVisitedCount++;
            var sb = new StringBuilder();
            sb.AppendLine($"—— {nodeId} ——");
            sb.AppendLine(node.Text.Trim());
            OnLog?.Invoke(sb.ToString());
            OnStateChanged?.Invoke();
        }

        public IReadOnlyList<ChoiceOption> GetChoicesForCurrentNode()
        {
            var list = new List<ChoiceOption>();
            foreach (var v in GetChoiceViewsForCurrentNode())
            {
                if (v.CanChoose && v.Option != null)
                    list.Add(v.Option);
            }
            return list;
        }

        public IReadOnlyList<ChoiceView> GetChoiceViewsForCurrentNode()
        {
            if (!_nodes.TryGetValue(CurrentNodeId, out var node))
                return Array.Empty<ChoiceView>();
            var list = new List<ChoiceView>();
            foreach (var c in node.Choices)
            {
                var canChoose = EvaluateChoiceAvailability(c, out var lockedHint);
                list.Add(new ChoiceView
                {
                    Option = c,
                    CanChoose = canChoose,
                    LockedHint = lockedHint
                });
            }
            return list;
        }

        bool EvaluateChoiceAvailability(ChoiceOption choice, out string lockedHint)
        {
            lockedHint = null;
            if (choice == null)
            {
                lockedHint = "选项无效。";
                return false;
            }

            if (!string.IsNullOrEmpty(choice.RequiresItemId) && !_player.HasItem(choice.RequiresItemId))
            {
                var itemLabel = choice.RequiresItemId;
                if (EquipmentCatalog.TryGet(choice.RequiresItemId, out var def))
                    itemLabel = def.Name;
                lockedHint = $"需要物品：{itemLabel}";
                return false;
            }

            if (choice.RequiresInsightSum > 0)
            {
                var sum = _player.GetStat(StatId.洞察) + _player.GetStat(StatId.机巧);
                if (sum < choice.RequiresInsightSum)
                {
                    lockedHint = $"需要洞察+机巧 ≥ {choice.RequiresInsightSum}（当前 {sum}）";
                    return false;
                }
            }

            return true;
        }

        public void Choose(ChoiceOption choice)
        {
            if (choice == null)
                return;

            if (!EvaluateChoiceAvailability(choice, out var lockedHint))
            {
                OnLog?.Invoke($"【条件不足】{lockedHint ?? "当前无法选择该项。"}");
                OnStateChanged?.Invoke();
                return;
            }

            _player.RunChoicesCount++;

            if (!string.IsNullOrEmpty(choice.GrantItemId))
                _player.AddItem(choice.GrantItemId);

            if (choice.GrantXp > 0)
            {
                if (_player.GrantXp(choice.GrantXp, out var lvlMsg) && !string.IsNullOrEmpty(lvlMsg))
                    OnLog?.Invoke(lvlMsg);
                OnLog?.Invoke($"获得经验 +{choice.GrantXp}。");
            }

            string nextId;
            if (choice.Check != null)
            {
                int stat = _player.GetStat(choice.Check.Stat);
                int roll = _rng.Next(1, 11);
                int total = stat + roll;
                bool ok = total >= choice.Check.Threshold;
                OnLog?.Invoke(
                    $"【检定·{choice.Check.Stat}】属性 {stat} + 掷骰 {roll} = {total}，需要 ≥{choice.Check.Threshold} → {(ok ? "成功" : "失败")}");
                nextId = ok ? choice.Check.SuccessNodeId : choice.Check.FailNodeId;
            }
            else
                nextId = choice.NextNodeId;

            if (string.IsNullOrEmpty(nextId))
            {
                OnLog?.Invoke("【剧终】");
                OnStateChanged?.Invoke();
                return;
            }

            if (nextId == "start")
            {
                _player.ResetForNewGame();
                OnNewRunStarted?.Invoke();
            }

            CurrentNodeId = nextId;
            EmitNode(nextId);
        }

        public void ApplyEquipFromDrag(string itemId, string slotKey)
        {
            if (_player.TryEquipToSlot(itemId, slotKey, out var msg))
                OnLog?.Invoke(msg);
            else
                OnLog?.Invoke(msg ?? "无法装备。");
            OnStateChanged?.Invoke();
        }

        public void ApplyUnequipFromDrag(string slotKey)
        {
            if (_player.TryUnequipSlot(slotKey, out var msg))
                OnLog?.Invoke(msg);
            else
                OnLog?.Invoke(msg ?? "无法卸下。");
            OnStateChanged?.Invoke();
        }

        public void ApplyConsumeFromDrag(string itemId)
        {
            if (_player.TryConsumeItem(itemId, out var msg))
                OnLog?.Invoke(msg);
            else
                OnLog?.Invoke(msg ?? "无法使用。");
            OnStateChanged?.Invoke();
        }

        public void LogInvalidUseZone()
        {
            OnLog?.Invoke("该物品无法在此使用。");
            OnStateChanged?.Invoke();
        }

        /// <summary>拖放无效时仅刷新界面，不改数据。</summary>
        public void RefreshUiOnly() => OnStateChanged?.Invoke();
    }
}
