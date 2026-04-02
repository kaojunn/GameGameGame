using UnityEngine;

/// <summary>
/// 横向位置固定，点击鼠标向上冲量；重力下落，用于类 Flappy 玩法。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BirdFlap : MonoBehaviour
{
    [SerializeField] float fixedX = -2.5f;
    [SerializeField] float jumpImpulse = 5.5f;

    Rigidbody _rb;
    FlappyDeath _death;
    FlappyAudio _audio;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _death = GetComponent<FlappyDeath>();
        _rb.useGravity = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    void Start()
    {
        _audio = FindFirstObjectByType<FlappyAudio>();
    }

    void Update()
    {
        if (_death != null && _death.IsDead)
            return;
        if (FlappyUI.IsGameplayPaused())
            return;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            _rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            if (_audio != null)
                _audio.PlayFlap();
        }
    }

    void FixedUpdate()
    {
        if (_death != null && _death.IsDead)
            return;
        var p = transform.position;
        p.x = fixedX;
        transform.position = p;
        var v = _rb.linearVelocity;
        v.x = 0f;
        _rb.linearVelocity = v;
    }
}
