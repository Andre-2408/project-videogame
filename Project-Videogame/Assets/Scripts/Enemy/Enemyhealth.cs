using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Salud")]
    public int maxHits = 3;

    [Header("Muerte")]
    [SerializeField] private GameObject deathAnimation; // arrastra el objeto de muerte aquí
    [SerializeField] private float deathDelay = 1f;     // tiempo antes de destruir

    private int _currentHits;
    private HitEffect _hitEffect;
    private bool _isDead = false;

    void Start()
    {
        _currentHits = maxHits;
        _hitEffect = GetComponent<HitEffect>();

        if (deathAnimation != null)
            deathAnimation.SetActive(false);
    }

    public void TakeDamage(float attackerX = 0f)
    {
        if (_isDead) return;

        _currentHits--;

        if (_hitEffect != null)
            _hitEffect.TakeHit(attackerX - transform.position.x);

        if (_currentHits <= 0)
            StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        _isDead = true;

        // Bloquear movimiento
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        var ai = GetComponent<EnemyAI>();
        if (ai != null)
            ai.enabled = false;

        // Ocultar sprites normales
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;

        // Activar animación de muerte
        if (deathAnimation != null)
        {
            deathAnimation.transform.position = transform.position;
            deathAnimation.SetActive(true);
        }

        yield return new WaitForSeconds(deathDelay);

        Destroy(gameObject);
    }
}