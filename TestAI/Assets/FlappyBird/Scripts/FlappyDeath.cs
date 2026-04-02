using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class FlappyDeath : MonoBehaviour
{
    bool _dead;

    public bool IsDead => _dead;

    void OnCollisionEnter(Collision other)
    {
        if (_dead)
            return;
        if (other.collider.GetComponentInParent<ScrollLeft>() != null ||
            other.gameObject.name.Contains("Pillar"))
            Die();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_dead)
            return;
        if (other.gameObject.name == "DeathZone")
            Die();
    }

    void Die()
    {
        _dead = true;

        var score = FindFirstObjectByType<FlappyScoreManager>();
        if (score != null)
            score.FreezeOnDeath();

        var flap = GetComponent<BirdFlap>();
        if (flap != null)
            flap.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        var audio = FindFirstObjectByType<FlappyAudio>();
        if (audio != null)
            audio.OnPlayerDied();

        var ui = FindFirstObjectByType<FlappyUI>();
        if (ui != null)
            ui.ShowGameOver();
    }

    public void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
