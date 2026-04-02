using UnityEngine;

public class FlappyUI : MonoBehaviour
{
    bool _show;
    FlappyDeath _death;
    FlappyScoreManager _score;

    void Awake()
    {
        _death = FindFirstObjectByType<FlappyDeath>();
        _score = FindFirstObjectByType<FlappyScoreManager>();
    }

    public void ShowGameOver()
    {
        _show = true;
    }

    void OnGUI()
    {
        const int w = 420;
        const int h = 132;
        var r = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        GUI.Label(new Rect(12, 12, 500, 40), "Flappy：鼠标左键 / 空格 = 跳跃");

        if (!_show)
            return;

        GUI.Box(r, GUIContent.none);
        var scoreLine = "";
        if (_score != null)
            scoreLine = $"本局 {_score.LastRunScore}    最佳 {_score.BestScore}" +
                        (_score.WasNewRecord ? "    新纪录！" : "") + "\n";
        GUI.Label(new Rect(r.x + 16, r.y + 12, w - 32, 44), scoreLine + "碰到绿色障碍了！按 R 重新开始");
        if (GUI.Button(new Rect(r.x + 16, r.y + 68, w - 32, 40), "重新开始 (R)") && _death != null)
            _death.ReloadScene();
    }

    void Update()
    {
        if (_show && Input.GetKeyDown(KeyCode.R) && _death != null)
            _death.ReloadScene();
    }
}
