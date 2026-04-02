using UnityEngine;

/// <summary>
/// 平滑跟随小鸟，保持构图稳定。
/// </summary>
public class CameraFollowBird : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(2.8f, 0f, -10f);
    [SerializeField] float smooth = 4f;

    void LateUpdate()
    {
        if (target == null)
            return;
        var want = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
