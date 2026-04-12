using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Salud")]
    public int maxHits = 3;

    private int _currentHits;

    void Start()
    {
        _currentHits = maxHits;
    }

    public void TakeDamage()
    {
        _currentHits--;

        Debug.Log($"[EnemyHealth] Golpe recibido. Hits restantes: {_currentHits}");

        if (_currentHits <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("[EnemyHealth] Enemigo eliminado.");
        Destroy(gameObject);
    }
}