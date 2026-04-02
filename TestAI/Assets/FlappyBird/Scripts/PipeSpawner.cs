using UnityEngine;

/// <summary>
/// 用立方体几何体拼出上下绿色障碍，带随机缺口高度。
/// </summary>
public class PipeSpawner : MonoBehaviour
{
    [SerializeField] Material obstacleMaterial;
    [SerializeField] float spawnInterval = 2.1f;
    [SerializeField] float gapHalfExtent = 1.15f;
    [SerializeField] float pillarThickness = 1.4f;
    [SerializeField] float pillarHeight = 12f;
    [SerializeField] float spawnX = 9f;
    [SerializeField] float randomYMin = -2.2f;
    [SerializeField] float randomYMax = 2.5f;
    [SerializeField] float destroyX = -14f;

    float _timer;

    void Start()
    {
        _timer = spawnInterval;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < spawnInterval)
            return;
        _timer = 0f;
        SpawnPair();
    }

    void SpawnPair()
    {
        float centerY = Random.Range(randomYMin, randomYMax);
        var root = new GameObject("PipePair");
        root.transform.position = Vector3.zero;
        root.AddComponent<ScrollLeft>();

        float halfGap = gapHalfExtent;
        float halfPillar = pillarHeight * 0.5f;

        // 上柱：中心在缺口上沿之上
        CreatePillar(root.transform, new Vector3(spawnX, centerY + halfGap + halfPillar, 0f));
        // 下柱：中心在缺口下沿之下
        CreatePillar(root.transform, new Vector3(spawnX, centerY - halfGap - halfPillar, 0f));

        CreateScoreGate(root.transform, centerY);

        var cull = root.AddComponent<DestroyWhenPastX>();
        cull.Initialize(destroyX);
    }

    void CreatePillar(Transform parent, Vector3 localPos)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Pillar";
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPos;
        cube.transform.localScale = new Vector3(pillarThickness, pillarHeight, pillarThickness);

        if (obstacleMaterial != null)
        {
            var r = cube.GetComponent<MeshRenderer>();
            r.sharedMaterial = obstacleMaterial;
        }

        var col = cube.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        var bc = cube.AddComponent<BoxCollider>();
        bc.size = Vector3.one;
        bc.center = Vector3.zero;
    }

    void CreateScoreGate(Transform parent, float centerY)
    {
        var go = new GameObject("ScoreGate");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(spawnX, centerY, 0f);
        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        float innerGap = gapHalfExtent * 2f - 0.28f;
        box.size = new Vector3(0.45f, Mathf.Max(0.55f, innerGap), 2.6f);
        go.AddComponent<PipeScoreGate>();
    }
}
