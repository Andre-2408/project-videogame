using UnityEngine;

/// <summary>
/// Spawner de enemigos. Asigna aleatoriamente el tipo de arma
/// antes de que el enemigo se active, para que arranque directo
/// con la animacion correcta sin ningun flash o corte.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Probabilidad de que el enemigo tenga cuchillo (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float knifeProbability = 0.5f;

    public GameObject SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return null;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // Asignar arma ANTES de que Start() se ejecute
        EnemyAnimationController animCtrl = enemy.GetComponent<EnemyAnimationController>();
        if (animCtrl != null)
        {
            EnemyAnimationController.EnemyWeapon weapon = Random.value < knifeProbability
                ? EnemyAnimationController.EnemyWeapon.Knife
                : EnemyAnimationController.EnemyWeapon.Gun;

            animCtrl.Initialize(weapon);
        }

        return enemy;
    }
}
