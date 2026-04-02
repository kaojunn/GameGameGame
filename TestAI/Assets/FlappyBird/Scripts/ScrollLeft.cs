using UnityEngine;

/// <summary>
/// 障碍整体向左移动。
/// </summary>
public class ScrollLeft : MonoBehaviour
{
    [SerializeField] float speed = 4f;

    void Update()
    {
        transform.position += Vector3.left * (speed * Time.deltaTime);
    }
}
