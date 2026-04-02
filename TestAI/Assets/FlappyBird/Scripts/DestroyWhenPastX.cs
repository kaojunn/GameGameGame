using UnityEngine;

public class DestroyWhenPastX : MonoBehaviour
{
    float _threshold;

    public void Initialize(float thresholdX)
    {
        _threshold = thresholdX;
    }

    void Update()
    {
        if (transform.position.x < _threshold)
            Destroy(gameObject);
    }
}
