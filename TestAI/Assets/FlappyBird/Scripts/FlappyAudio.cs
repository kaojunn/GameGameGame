using UnityEngine;

/// <summary>
/// BGM 循环 + 一次性音效。素材：Kenney CC0（Impact Sounds / Music Jingles），见 Audio 目录注释。
/// </summary>
public class FlappyAudio : MonoBehaviour
{
    [SerializeField] AudioClip bgmClip;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip flapClip;
    [SerializeField] [Range(0f, 1f)] float bgmVolume = 0.45f;
    [SerializeField] [Range(0f, 1f)] float hitVolume = 0.85f;
    [SerializeField] [Range(0f, 1f)] float flapVolume = 0.55f;
    [SerializeField] [Range(0f, 1f)] float scorePingVolume = 0.22f;

    AudioSource _music;
    AudioSource _sfx;

    void Awake()
    {
        var sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            _music = sources[0];
            _sfx = sources[1];
        }
        else if (sources.Length == 1)
        {
            _music = sources[0];
            _sfx = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            _music = gameObject.AddComponent<AudioSource>();
            _sfx = gameObject.AddComponent<AudioSource>();
        }

        _music.loop = true;
        _music.playOnAwake = false;
        _music.volume = bgmVolume;
        _sfx.playOnAwake = false;
    }

    void Start()
    {
        if (bgmClip != null)
        {
            _music.clip = bgmClip;
            _music.Play();
        }
    }

    public void PlayFlap()
    {
        if (flapClip == null || _sfx == null)
            return;
        _sfx.PlayOneShot(flapClip, flapVolume);
    }

    public void PlayScorePing()
    {
        if (flapClip == null || _sfx == null)
            return;
        _sfx.PlayOneShot(flapClip, scorePingVolume);
    }

    public void OnPlayerDied()
    {
        if (_music != null && _music.isPlaying)
            _music.Stop();
        if (hitClip != null && _sfx != null)
            _sfx.PlayOneShot(hitClip, hitVolume);
    }
}
