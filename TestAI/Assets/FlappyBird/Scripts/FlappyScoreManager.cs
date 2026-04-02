using UnityEngine;

/// <summary>
/// 穿缝计分与最高分（PlayerPrefs）。死亡时冻结本局分数并可能刷新最佳。
/// </summary>
public class FlappyScoreManager : MonoBehaviour
{
    const string PrefsKey = "FlappyBirdBestScore";

    int _current;
    int _best;
    bool _frozen;
    bool _newRecord;

    public int CurrentScore => _current;
    public int BestScore => _best;
    public int LastRunScore { get; private set; }
    public bool WasNewRecord => _newRecord;
    public bool IsRunFrozen => _frozen;

    void Awake()
    {
        _best = PlayerPrefs.GetInt(PrefsKey, 0);
    }

    void Start()
    {
        _current = 0;
        _frozen = false;
        _newRecord = false;
        LastRunScore = 0;
    }

    public void AddPipePassed()
    {
        if (_frozen)
            return;
        _current++;
    }

    public void FreezeOnDeath()
    {
        if (_frozen)
            return;
        _frozen = true;
        LastRunScore = _current;
        if (_current > _best)
        {
            _best = _current;
            _newRecord = true;
            PlayerPrefs.SetInt(PrefsKey, _best);
            PlayerPrefs.Save();
        }
        else
            _newRecord = false;
    }
}
