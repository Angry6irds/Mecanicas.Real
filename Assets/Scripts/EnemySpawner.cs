using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform turret;
    public GameObject enemyPrefab;
    public float minRadius = 15f;
    public float maxRadius = 25f;
    public int initialSpawn = 5;
    public float spawnInterval = 2.5f;
    public int maxAlive = 20;

    float timer;

    void Start()
    {
        for (int i = 0; i < initialSpawn; i++) SpawnOne();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (Object.FindObjectsByType<EnemyChaser>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length < maxAlive)
                SpawnOne();
        }
    }

    void SpawnOne()
    {
        if (!turret || !enemyPrefab) return;
        float ang = Random.Range(0f, Mathf.PI * 2f);
        float r = Random.Range(minRadius, maxRadius);
        Vector3 pos = turret.position + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
        if (Physics.Raycast(pos + Vector3.up * 30f, Vector3.down, out var hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            pos = hit.point;
        var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        var ch = go.GetComponent<EnemyChaser>();
        if (ch) ch.target = turret;
    }
}