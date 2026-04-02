using UnityEngine;

/// <summary>
/// 每对管道中间的一次性得分触发器。
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PipeScoreGate : MonoBehaviour
{
    bool _used;

    void OnTriggerEnter(Collider other)
    {
        if (_used)
            return;

        var death = other.GetComponentInParent<FlappyDeath>();
        if (death == null || death.IsDead)
            return;

        var score = FindFirstObjectByType<FlappyScoreManager>();
        if (score == null || score.IsRunFrozen)
            return;

        _used = true;
        score.AddPipePassed();
        FindFirstObjectByType<FlappyAudio>()?.PlayScorePing();
    }
}
