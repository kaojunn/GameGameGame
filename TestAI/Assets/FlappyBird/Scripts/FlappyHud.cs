using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 运行时生成 Canvas，显示当前分与历史最佳（与黄/蓝主题协调的亮色文字）。
/// </summary>
public class FlappyHud : MonoBehaviour
{
    FlappyScoreManager _score;
    Text _hudText;

    void Start()
    {
        _score = FindFirstObjectByType<FlappyScoreManager>();
        BuildCanvas();
    }

    void BuildCanvas()
    {
        var canvasGo = new GameObject("FlappyHudCanvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        var textGo = new GameObject("HudText");
        textGo.transform.SetParent(canvasGo.transform, false);
        var rt = textGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(28f, -24f);
        rt.sizeDelta = new Vector2(480f, 100f);

        _hudText = textGo.AddComponent<Text>();
        _hudText.font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "PingFang SC", "Helvetica" }, 28);
        _hudText.fontSize = 28;
        _hudText.fontStyle = FontStyle.Bold;
        _hudText.color = new Color(1f, 0.94f, 0.4f);
        _hudText.alignment = TextAnchor.UpperLeft;
    }

    void LateUpdate()
    {
        if (_hudText == null || _score == null)
            return;
        int shown = _score.IsRunFrozen ? _score.LastRunScore : _score.CurrentScore;
        _hudText.text = $"分数 {shown}\n最佳 {_score.BestScore}";
    }
}
