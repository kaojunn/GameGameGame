using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NorthernTown2026
{
    /// <summary>可拖动的装备小卡片；拖动时射线穿透以检测槽位/背包区域。</summary>
    public class DraggableEquipmentCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public string ItemId;
        public bool FromSlot;
        public string SlotKey;
        public TextAdventureBootstrap Host;
        public RectTransform DragLayer;

        CanvasGroup _cg;
        Canvas _rootCanvas;

        CanvasGroup EnsureCanvasGroup()
        {
            if (_cg == null)
                _cg = GetComponent<CanvasGroup>();
            if (_cg == null)
                _cg = gameObject.AddComponent<CanvasGroup>();
            return _cg;
        }

        void Awake()
        {
            EnsureCanvasGroup();
            _rootCanvas = GetComponentInParent<Canvas>();
        }

        void OnEnable()
        {
            EnsureCanvasGroup();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var cg = EnsureCanvasGroup();
            cg.blocksRaycasts = false;
            cg.alpha = 0.92f;
            var rt = (RectTransform)transform;
            var world = rt.position;
            rt.SetParent(DragLayer, false);
            rt.position = world;
            _rootCanvas = GetComponentInParent<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            var rt = (RectTransform)transform;
            float s = _rootCanvas != null ? _rootCanvas.scaleFactor : 1f;
            rt.anchoredPosition += eventData.delta / s;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (this == null)
                return;
            var cg = EnsureCanvasGroup();
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
            if (Host != null)
                Host.HandleEquipmentCardDrop(this, eventData);
        }
    }
}
