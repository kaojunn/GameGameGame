using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NorthernTown2026
{
    /// <summary>空场景入口：EventSystem + 运行时 UGUI + 文字冒险引擎。</summary>
    public class TextAdventureBootstrap : MonoBehaviour
    {
        TextAdventureEngine _engine;
        Text _logText;
        ScrollRect _scroll;
        RectTransform _choicesRoot;
        Text _statsText;
        RectTransform _inventoryGridRoot;
        RectTransform _inventoryZoneRt;
        RectTransform _consumableUseRt;
        RectTransform _dragLayer;
        readonly Dictionary<string, Transform> _slotCardParents = new Dictionary<string, Transform>();
        readonly Dictionary<string, RectTransform> _slotHitRects = new Dictionary<string, RectTransform>();

        static Font UiFont(int size) =>
            Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Arial", "Helvetica" }, size);

        void Awake()
        {
            EnsureEventSystem();
            BuildUi();
            _engine = new TextAdventureEngine(NorthernTownStoryContent.BuildGraph());
            _engine.OnLog += AppendLog;
            _engine.OnStateChanged += RefreshAll;
            _engine.OnNewRunStarted += ClearLog;
            _engine.Start();
        }

        void OnDestroy()
        {
            if (_engine == null)
                return;
            _engine.OnLog -= AppendLog;
            _engine.OnStateChanged -= RefreshAll;
            _engine.OnNewRunStarted -= ClearLog;
        }

        void ClearLog()
        {
            if (_logText != null)
                _logText.text = string.Empty;
            SyncNarrativeContentHeight();
        }

        void AppendLog(string chunk)
        {
            if (_logText == null)
                return;
            _logText.text += chunk + "\n\n";
            Canvas.ForceUpdateCanvases();
            SyncNarrativeContentHeight();
            StartCoroutine(ScrollNarrativeToBottomAfterLayout());
        }

        /// <summary>
        /// 用 TextGenerator 显式计算文本高度并设置 Content 尺寸；仅靠 ContentSizeFitter 时 ScrollRect 常无法滚动。
        /// </summary>
        void SyncNarrativeContentHeight()
        {
            if (_scroll == null || _logText == null || _scroll.viewport == null || _scroll.content == null)
                return;
            var vp = _scroll.viewport;
            float innerW = Mathf.Max(8f, vp.rect.width - 16f);
            var logRt = _logText.rectTransform;
            logRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, innerW);
            var settings = _logText.GetGenerationSettings(new Vector2(innerW, 0f));
            float h = _logText.cachedTextGeneratorForLayout.GetPreferredHeight(_logText.text, settings);
            h = Mathf.Max(h + 8f, 32f);
            logRt.sizeDelta = new Vector2(0f, h);
            var contentRt = _scroll.content;
            contentRt.sizeDelta = new Vector2(0f, h);
        }

        IEnumerator ScrollNarrativeToBottomAfterLayout()
        {
            yield return new WaitForEndOfFrame();
            if (_scroll == null || _scroll.content == null || _scroll.viewport == null)
                yield break;
            SyncNarrativeContentHeight();
            Canvas.ForceUpdateCanvases();
            _scroll.StopMovement();
            _scroll.verticalNormalizedPosition = 0f;
        }

        void RefreshAll()
        {
            if (_engine == null || _statsText == null)
                return;
            _statsText.text = _engine.Player.FormatStatusBlock() + "\n" + _engine.FormatMetaProgressBlock();
            RebuildEquipmentCards();
            RebuildChoiceButtons();
        }

        public void HandleEquipmentCardDrop(DraggableEquipmentCard card, PointerEventData eventData)
        {
            if (_engine == null || card == null)
                return;
            if (!EquipmentCatalog.TryGet(card.ItemId, out var def))
            {
                _engine.RefreshUiOnly();
                return;
            }

            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            ConsumableUseZone useZone = null;
            foreach (var r in results)
            {
                var u = r.gameObject.GetComponentInParent<ConsumableUseZone>();
                if (u != null)
                {
                    useZone = u;
                    break;
                }
            }

            if (useZone == null && _consumableUseRt != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_consumableUseRt, pos, cam))
                useZone = _consumableUseRt.GetComponent<ConsumableUseZone>();

            if (useZone != null)
            {
                if (def.Consumable)
                    _engine.ApplyConsumeFromDrag(card.ItemId);
                else
                    _engine.LogInvalidUseZone();
                return;
            }

            EquipSlotDropZone slotZone = null;
            foreach (var r in results)
            {
                var z = r.gameObject.GetComponentInParent<EquipSlotDropZone>();
                if (z != null)
                {
                    slotZone = z;
                    break;
                }
            }

            if (slotZone == null)
            {
                foreach (var kv in _slotHitRects)
                {
                    if (kv.Value != null &&
                        RectTransformUtility.RectangleContainsScreenPoint(kv.Value, pos, cam))
                    {
                        slotZone = kv.Value.GetComponent<EquipSlotDropZone>();
                        break;
                    }
                }
            }

            InventoryBackpackZone invZone = null;
            foreach (var r in results)
            {
                var inv = r.gameObject.GetComponentInParent<InventoryBackpackZone>();
                if (inv != null)
                {
                    invZone = inv;
                    break;
                }
            }

            if (invZone == null && _inventoryZoneRt != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_inventoryZoneRt, pos, cam))
                invZone = _inventoryZoneRt.GetComponent<InventoryBackpackZone>();

            if (slotZone != null)
            {
                _engine.ApplyEquipFromDrag(card.ItemId, slotZone.SlotKey);
                return;
            }

            if (invZone != null)
            {
                if (card.FromSlot && !string.IsNullOrEmpty(card.SlotKey))
                    _engine.ApplyUnequipFromDrag(card.SlotKey);
                else
                    _engine.RefreshUiOnly();
                return;
            }

            _engine.RefreshUiOnly();
        }

        void RebuildEquipmentCards()
        {
            if (_dragLayer != null)
            {
                for (var i = _dragLayer.childCount - 1; i >= 0; i--)
                    Destroy(_dragLayer.GetChild(i).gameObject);
            }

            if (_inventoryGridRoot != null)
            {
                for (var i = _inventoryGridRoot.childCount - 1; i >= 0; i--)
                    Destroy(_inventoryGridRoot.GetChild(i).gameObject);
            }

            foreach (var kv in _slotCardParents)
            {
                if (kv.Value == null)
                    continue;
                for (var i = kv.Value.childCount - 1; i >= 0; i--)
                    Destroy(kv.Value.GetChild(i).gameObject);
            }

            if (_engine == null)
                return;

            var player = _engine.Player;
            foreach (var slot in new[] { "终端", "外套", "饰品" })
            {
                if (!player.EquippedBySlot.TryGetValue(slot, out var id))
                    continue;
                if (!EquipmentCatalog.TryGet(id, out _))
                    continue;
                if (_slotCardParents.TryGetValue(slot, out var parent) && parent != null)
                    CreateEquipmentCard(id, parent, true, slot);
            }

            foreach (var id in GetUnequippedEquipableIds(player))
            {
                if (_inventoryGridRoot != null)
                    CreateEquipmentCard(id, _inventoryGridRoot, false, null);
            }

            foreach (var id in player.InventoryItemIds)
            {
                if (EquipmentCatalog.TryGet(id, out _))
                    continue;
                CreateQuestOrUnknownCard(id, _inventoryGridRoot);
            }
        }

        static IEnumerable<string> GetUnequippedEquipableIds(PlayerState player)
        {
            var equipped = new HashSet<string>(player.EquippedBySlot.Values);
            foreach (var id in player.InventoryItemIds)
            {
                if (!EquipmentCatalog.TryGet(id, out _))
                    continue;
                if (!equipped.Contains(id))
                    yield return id;
            }
        }

        void CreateEquipmentCard(string itemId, Transform parent, bool fromSlot, string slotKey)
        {
            if (!EquipmentCatalog.TryGet(itemId, out var def))
                return;

            var go = new GameObject("Card_" + itemId);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(86f, 96f);

            var img = go.AddComponent<Image>();
            img.color = def.Consumable
                ? new Color(0.18f, 0.26f, 0.22f, 1f)
                : new Color(0.2f, 0.24f, 0.34f, 1f);
            img.raycastTarget = true;

            var drag = go.AddComponent<DraggableEquipmentCard>();
            drag.ItemId = itemId;
            drag.FromSlot = fromSlot;
            drag.SlotKey = slotKey;
            drag.Host = this;
            drag.DragLayer = _dragLayer;

            var labelGo = new GameObject("Name");
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(1f, 1f);
            lrt.offsetMin = new Vector2(6f, 22f);
            lrt.offsetMax = new Vector2(-6f, -6f);
            var t = labelGo.AddComponent<Text>();
            t.font = UiFont(14);
            t.fontSize = 14;
            t.color = new Color(0.92f, 0.94f, 0.98f);
            t.alignment = TextAnchor.UpperCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.text = def.Name;
            t.raycastTarget = false;

            var slotGo = new GameObject("SlotHint");
            slotGo.transform.SetParent(go.transform, false);
            var srt = slotGo.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(1f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.anchoredPosition = new Vector2(0f, 4f);
            srt.sizeDelta = new Vector2(0f, 18f);
            var st = slotGo.AddComponent<Text>();
            st.font = UiFont(12);
            st.fontSize = 12;
            st.color = new Color(0.65f, 0.75f, 0.88f);
            st.alignment = TextAnchor.LowerCenter;
            st.text = def.Consumable ? "消耗品" : def.Slot;
            st.raycastTarget = false;
        }

        void CreateQuestOrUnknownCard(string itemId, Transform parent)
        {
            if (parent == null)
                return;
            var go = new GameObject("Quest_" + itemId);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(86f, 40f);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.16f, 0.2f, 1f);
            img.raycastTarget = false;
            var tGo = new GameObject("T");
            tGo.transform.SetParent(go.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;
            var t = tGo.AddComponent<Text>();
            t.font = UiFont(13);
            t.fontSize = 13;
            t.color = new Color(0.7f, 0.72f, 0.76f);
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.text = itemId;
        }

        void RebuildChoiceButtons()
        {
            foreach (Transform c in _choicesRoot)
                Destroy(c.gameObject);

            foreach (var opt in _engine.GetChoicesForCurrentNode())
            {
                var btnGo = new GameObject("Choice");
                btnGo.transform.SetParent(_choicesRoot, false);
                var img = btnGo.AddComponent<Image>();
                img.color = new Color(0.18f, 0.22f, 0.32f, 1f);
                img.raycastTarget = true;
                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = img;
                var rt = btnGo.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0f, 48f);
                var btnLe = btnGo.AddComponent<LayoutElement>();
                btnLe.minHeight = 48f;
                btnLe.preferredHeight = 48f;
                btnLe.flexibleWidth = 1f;

                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(btnGo.transform, false);
                var labelRt = labelGo.AddComponent<RectTransform>();
                labelRt.anchorMin = Vector2.zero;
                labelRt.anchorMax = Vector2.one;
                labelRt.offsetMin = new Vector2(12f, 4f);
                labelRt.offsetMax = new Vector2(-12f, -4f);
                var t = labelGo.AddComponent<Text>();
                t.font = UiFont(20);
                t.fontSize = 20;
                t.color = new Color(0.92f, 0.94f, 0.98f);
                t.alignment = TextAnchor.MiddleLeft;
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                t.text = opt.Text;
                t.raycastTarget = false;

                var captured = opt;
                btn.onClick.AddListener(() => _engine.Choose(captured));
            }
        }

        /// <summary>
        /// 使用 StandaloneInputModule：项目在 Player Settings 中常为「新旧输入并存」，
        /// 此时未配置 UI Input Actions 的 InputSystemUIInputModule 会导致按钮完全无响应。
        /// </summary>
        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        void BuildUi()
        {
            const float bottomMargin = 24f;
            const float choiceStripHeight = 280f;
            const float gapAboveChoices = 12f;

            var canvasGo = new GameObject("NorthernTownCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var fullRoot = new GameObject("Root");
            fullRoot.transform.SetParent(canvasGo.transform, false);
            var fullRt = fullRoot.AddComponent<RectTransform>();
            fullRt.anchorMin = Vector2.zero;
            fullRt.anchorMax = Vector2.one;
            fullRt.offsetMin = Vector2.zero;
            fullRt.offsetMax = Vector2.zero;

            var choiceArea = new GameObject("Choices");
            choiceArea.transform.SetParent(fullRoot.transform, false);
            var choiceRt = choiceArea.AddComponent<RectTransform>();
            choiceRt.anchorMin = new Vector2(0f, 0f);
            choiceRt.anchorMax = new Vector2(1f, 0f);
            choiceRt.pivot = new Vector2(0.5f, 0f);
            choiceRt.anchoredPosition = new Vector2(0f, bottomMargin);
            choiceRt.sizeDelta = new Vector2(-48f, choiceStripHeight);
            var choiceBg = choiceArea.AddComponent<Image>();
            choiceBg.color = new Color(0.06f, 0.07f, 0.1f, 0.98f);
            var choiceV = choiceArea.AddComponent<VerticalLayoutGroup>();
            choiceV.padding = new RectOffset(12, 12, 12, 12);
            choiceV.spacing = 8f;
            choiceV.childAlignment = TextAnchor.UpperCenter;
            choiceV.childControlHeight = true;
            choiceV.childControlWidth = true;
            choiceV.childForceExpandHeight = false;
            choiceV.childForceExpandWidth = true;
            _choicesRoot = choiceRt;

            var topRow = new GameObject("TopRow");
            topRow.transform.SetParent(fullRoot.transform, false);
            var topRt = topRow.AddComponent<RectTransform>();
            topRt.anchorMin = Vector2.zero;
            topRt.anchorMax = Vector2.one;
            topRt.offsetMin = new Vector2(24f, bottomMargin + choiceStripHeight + gapAboveChoices);
            topRt.offsetMax = new Vector2(-24f, -24f);
            var topH = topRow.AddComponent<HorizontalLayoutGroup>();
            topH.spacing = 16f;
            topH.childForceExpandHeight = true;
            topH.childForceExpandWidth = true;

            BuildLogPanel(topRow.transform);
            BuildStatsPanel(topRow.transform);

            var dragGo = new GameObject("DragLayer");
            dragGo.transform.SetParent(canvasGo.transform, false);
            _dragLayer = dragGo.AddComponent<RectTransform>();
            _dragLayer.anchorMin = Vector2.zero;
            _dragLayer.anchorMax = Vector2.one;
            _dragLayer.offsetMin = Vector2.zero;
            _dragLayer.offsetMax = Vector2.zero;
            var dragCv = dragGo.AddComponent<Canvas>();
            dragCv.overrideSorting = true;
            dragCv.sortingOrder = 300;
            dragGo.AddComponent<GraphicRaycaster>();
        }

        void BuildLogPanel(Transform parent)
        {
            const float scrollbarW = 20f;

            var panel = new GameObject("LogPanel");
            panel.transform.SetParent(parent, false);
            var panelLe = panel.AddComponent<LayoutElement>();
            panelLe.flexibleWidth = 2.2f;
            panelLe.flexibleHeight = 1f;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.11f, 0.14f, 1f);

            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(panel.transform, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(12f, 12f);
            scrollRt.offsetMax = new Vector2(-12f, -12f);
            var scrollLe = scrollGo.AddComponent<LayoutElement>();
            scrollLe.flexibleWidth = 1f;
            scrollLe.flexibleHeight = 1f;
            scrollLe.minHeight = 80f;
            _scroll = scrollGo.AddComponent<ScrollRect>();
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 32f;
            _scroll.inertia = true;
            _scroll.decelerationRate = 0.135f;
            _scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGo.transform, false);
            var vpRt = viewport.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = new Vector2(-scrollbarW, 0f);
            vpRt.sizeDelta = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            var vpImg = viewport.AddComponent<Image>();
            vpImg.color = new Color(1f, 1f, 1f, 0.02f);
            vpImg.raycastTarget = true;
            _scroll.viewport = vpRt;

            var scrollbarGo = new GameObject("Scrollbar Vertical");
            scrollbarGo.transform.SetParent(scrollGo.transform, false);
            var sbRt = scrollbarGo.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(1f, 0f);
            sbRt.anchorMax = new Vector2(1f, 1f);
            sbRt.pivot = new Vector2(1f, 0.5f);
            sbRt.sizeDelta = new Vector2(scrollbarW, 0f);
            sbRt.anchoredPosition = Vector2.zero;
            var trackImg = scrollbarGo.AddComponent<Image>();
            trackImg.color = new Color(0.14f, 0.15f, 0.19f, 1f);
            trackImg.raycastTarget = true;

            var sliding = new GameObject("Sliding Area");
            sliding.transform.SetParent(scrollbarGo.transform, false);
            var slidingRt = sliding.AddComponent<RectTransform>();
            slidingRt.anchorMin = Vector2.zero;
            slidingRt.anchorMax = Vector2.one;
            slidingRt.offsetMin = new Vector2(2f, 3f);
            slidingRt.offsetMax = new Vector2(-2f, -3f);

            var handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(sliding.transform, false);
            var handleRt = handleGo.AddComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0f, 0f);
            handleRt.anchorMax = new Vector2(1f, 1f);
            handleRt.pivot = new Vector2(0.5f, 0.5f);
            handleRt.sizeDelta = Vector2.zero;
            var handleImg = handleGo.AddComponent<Image>();
            handleImg.color = new Color(0.42f, 0.48f, 0.58f, 1f);
            handleImg.raycastTarget = true;

            var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = handleImg;
            scrollbar.handleRect = handleRt;
            _scroll.verticalScrollbar = scrollbar;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 100f);

            var logGo = new GameObject("LogText");
            logGo.transform.SetParent(content.transform, false);
            var logRt = logGo.AddComponent<RectTransform>();
            logRt.anchorMin = new Vector2(0f, 1f);
            logRt.anchorMax = new Vector2(1f, 1f);
            logRt.pivot = new Vector2(0.5f, 1f);
            logRt.sizeDelta = new Vector2(0f, 100f);
            _logText = logGo.AddComponent<Text>();
            _logText.font = UiFont(22);
            _logText.fontSize = 22;
            _logText.color = new Color(0.9f, 0.91f, 0.93f);
            _logText.alignment = TextAnchor.UpperLeft;
            _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _logText.verticalOverflow = VerticalWrapMode.Overflow;
            _logText.raycastTarget = false;

            _scroll.content = contentRt;
        }

        void BuildStatsPanel(Transform parent)
        {
            var panel = new GameObject("StatsPanel");
            panel.transform.SetParent(parent, false);
            var panelLe = panel.AddComponent<LayoutElement>();
            panelLe.flexibleWidth = 1f;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.13f, 0.16f, 1f);
            panelImg.raycastTarget = false;

            // 右侧整列单独 Canvas 排序，避免左侧叙事 ScrollRect/视口在屏幕空间上盖住右栏导致点击全被左边吃掉。
            var statsCanvas = panel.AddComponent<Canvas>();
            statsCanvas.overrideSorting = true;
            statsCanvas.sortingOrder = 20;
            panel.AddComponent<GraphicRaycaster>();

            var inner = new GameObject("Inner");
            inner.transform.SetParent(panel.transform, false);
            var innerRt = inner.AddComponent<RectTransform>();
            innerRt.anchorMin = Vector2.zero;
            innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = new Vector2(12f, 12f);
            innerRt.offsetMax = new Vector2(-12f, -12f);
            var innerV = inner.AddComponent<VerticalLayoutGroup>();
            innerV.spacing = 10f;
            innerV.childForceExpandHeight = false;
            innerV.childForceExpandWidth = true;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(inner.transform, false);
            var titleT = titleGo.AddComponent<Text>();
            titleT.font = UiFont(22);
            titleT.fontSize = 22;
            titleT.fontStyle = FontStyle.Bold;
            titleT.color = new Color(0.75f, 0.85f, 1f);
            titleT.alignment = TextAnchor.UpperLeft;
            titleT.text = "状态 · 装备";
            titleT.raycastTarget = false;

            var statsGo = new GameObject("Stats");
            statsGo.transform.SetParent(inner.transform, false);
            _statsText = statsGo.AddComponent<Text>();
            _statsText.font = UiFont(18);
            _statsText.fontSize = 18;
            _statsText.color = new Color(0.82f, 0.84f, 0.88f);
            _statsText.alignment = TextAnchor.UpperLeft;
            _statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _statsText.verticalOverflow = VerticalWrapMode.Overflow;
            _statsText.raycastTarget = false;
            var statsLe = statsGo.AddComponent<LayoutElement>();
            statsLe.flexibleHeight = 0f;
            statsLe.preferredHeight = 200f;

            var slotsTitle = new GameObject("SlotsTitle");
            slotsTitle.transform.SetParent(inner.transform, false);
            var stt = slotsTitle.AddComponent<Text>();
            stt.font = UiFont(17);
            stt.fontSize = 17;
            stt.color = new Color(0.72f, 0.82f, 0.95f);
            stt.alignment = TextAnchor.UpperLeft;
            stt.text = "装备槽（拖入对应卡片）";
            stt.raycastTarget = false;

            var slotsRow = new GameObject("SlotsRow");
            slotsRow.transform.SetParent(inner.transform, false);
            var slotsRowLe = slotsRow.AddComponent<LayoutElement>();
            slotsRowLe.preferredHeight = 148f;
            slotsRowLe.flexibleHeight = 0f;
            var slotsH = slotsRow.AddComponent<HorizontalLayoutGroup>();
            slotsH.spacing = 8f;
            slotsH.childAlignment = TextAnchor.UpperCenter;
            slotsH.childForceExpandHeight = true;
            slotsH.childForceExpandWidth = true;
            slotsH.padding = new RectOffset(0, 0, 0, 0);

            _slotCardParents.Clear();
            _slotHitRects.Clear();
            foreach (var slot in new[] { "终端", "外套", "饰品" })
            {
                var slotGo = new GameObject("Slot_" + slot);
                slotGo.transform.SetParent(slotsRow.transform, false);
                var slotLe = slotGo.AddComponent<LayoutElement>();
                slotLe.flexibleWidth = 1f;
                slotLe.minWidth = 88f;
                slotLe.preferredHeight = 140f;
                var slotV = slotGo.AddComponent<VerticalLayoutGroup>();
                slotV.spacing = 4f;
                slotV.childAlignment = TextAnchor.UpperCenter;
                slotV.childControlHeight = true;
                slotV.childControlWidth = true;

                var slotLabel = new GameObject("Label");
                slotLabel.transform.SetParent(slotGo.transform, false);
                var slt = slotLabel.AddComponent<Text>();
                slt.font = UiFont(14);
                slt.fontSize = 14;
                slt.color = new Color(0.75f, 0.8f, 0.9f);
                slt.alignment = TextAnchor.MiddleCenter;
                slt.text = slot;
                slt.raycastTarget = false;

                var hitArea = new GameObject("Hit");
                hitArea.transform.SetParent(slotGo.transform, false);
                var hitRt = hitArea.AddComponent<RectTransform>();
                hitRt.sizeDelta = new Vector2(0f, 110f);
                var hitLe = hitArea.AddComponent<LayoutElement>();
                hitLe.flexibleHeight = 1f;
                hitLe.minHeight = 100f;
                var hitImg = hitArea.AddComponent<Image>();
                hitImg.color = new Color(0.1f, 0.12f, 0.16f, 1f);
                hitImg.raycastTarget = true;
                var zone = hitArea.AddComponent<EquipSlotDropZone>();
                zone.SlotKey = slot;
                _slotHitRects[slot] = hitRt;

                var cardParent = new GameObject("CardAnchor");
                cardParent.transform.SetParent(hitArea.transform, false);
                var capRt = cardParent.AddComponent<RectTransform>();
                capRt.anchorMin = Vector2.zero;
                capRt.anchorMax = Vector2.one;
                capRt.offsetMin = new Vector2(4f, 4f);
                capRt.offsetMax = new Vector2(-4f, -4f);
                _slotCardParents[slot] = cardParent.transform;
            }

            var useTitle = new GameObject("UseTitle");
            useTitle.transform.SetParent(inner.transform, false);
            var utt = useTitle.AddComponent<Text>();
            utt.font = UiFont(17);
            utt.fontSize = 17;
            utt.color = new Color(0.75f, 0.88f, 0.72f);
            utt.alignment = TextAnchor.UpperLeft;
            utt.text = "使用（拖入消耗品）";
            utt.raycastTarget = false;

            var useRow = new GameObject("UseRow");
            useRow.transform.SetParent(inner.transform, false);
            var useRowLe = useRow.AddComponent<LayoutElement>();
            useRowLe.preferredHeight = 72f;
            useRowLe.flexibleHeight = 0f;
            var useImg = useRow.AddComponent<Image>();
            useImg.color = new Color(0.12f, 0.16f, 0.13f, 1f);
            useImg.raycastTarget = true;
            useRow.AddComponent<ConsumableUseZone>();
            _consumableUseRt = useRow.GetComponent<RectTransform>();

            var backpackTitle = new GameObject("BackpackTitle");
            backpackTitle.transform.SetParent(inner.transform, false);
            var btt = backpackTitle.AddComponent<Text>();
            btt.font = UiFont(17);
            btt.fontSize = 17;
            btt.color = new Color(0.7f, 0.8f, 0.75f);
            btt.alignment = TextAnchor.UpperLeft;
            btt.text = "背包（拖出已装备可卸下；松手在区外会归位）";
            btt.raycastTarget = false;

            var backpack = new GameObject("Backpack");
            backpack.transform.SetParent(inner.transform, false);
            var bpLe = backpack.AddComponent<LayoutElement>();
            bpLe.flexibleHeight = 1f;
            bpLe.minHeight = 140f;
            var bpImg = backpack.AddComponent<Image>();
            bpImg.color = new Color(0.1f, 0.11f, 0.14f, 1f);
            bpImg.raycastTarget = true;
            backpack.AddComponent<InventoryBackpackZone>();
            var grid = backpack.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(88f, 96f);
            grid.spacing = new Vector2(8f, 8f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            grid.padding = new RectOffset(8, 8, 8, 8);
            _inventoryGridRoot = backpack.GetComponent<RectTransform>();
            _inventoryZoneRt = _inventoryGridRoot;
        }
    }
}
