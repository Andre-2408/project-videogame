using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Pool genérico de balas. Coloca este script en un GameObject vacío "BulletPool" en la escena.
/// Asigna el prefab de bala del jugador y el prefab de bala enemiga en el Inspector.
/// </summary>
public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private GameObject enemyBulletPrefab;

    [Header("Tamaño inicial del pool")]
    [SerializeField] private int playerPoolSize = 20;
    [SerializeField] private int enemyPoolSize  = 30;

    private readonly Queue<GameObject> _playerPool = new Queue<GameObject>();
    private readonly Queue<GameObject> _enemyPool  = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Prewarm(playerBulletPrefab, _playerPool, playerPoolSize);
        Prewarm(enemyBulletPrefab,  _enemyPool,  enemyPoolSize);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── API pública ──────────────────────────────
    public GameObject GetPlayerBullet(Vector3 pos, Quaternion rot)
        => Get(_playerPool, playerBulletPrefab, pos, rot);

    public GameObject GetEnemyBullet(Vector3 pos, Quaternion rot)
        => Get(_enemyPool, enemyBulletPrefab, pos, rot);

    public void ReturnPlayerBullet(GameObject go) => Return(_playerPool, go);
    public void ReturnEnemyBullet(GameObject go)  => Return(_enemyPool,  go);

    // ── Internos ─────────────────────────────────
    private void Prewarm(GameObject prefab, Queue<GameObject> pool, int count)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    private GameObject Get(Queue<GameObject> pool, GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Dequeue();
        }
        else
        {
            // Pool vacío — crear uno extra
            go = Instantiate(prefab, transform);
        }
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }

    private void Return(Queue<GameObject> pool, GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);
        pool.Enqueue(go);
    }
}
