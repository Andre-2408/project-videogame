using UnityEngine;

/// <summary>
/// Onda de choque que viaja por el suelo cuando el gorila golpea.
/// Agrégale un SpriteRenderer, Rigidbody2D (Gravity Scale=0) y CircleCollider2D (Is Trigger).
/// </summary>
public class GorillaShockwave : MonoBehaviour
{
    public float speed = 6f;
    public float lifetime = 2f;
    public int damage = 1;

    private Rigidbody2D _rb;
    private float _direction = 1f;

    public void SetDirection(float dir)
    {
        _direction = dir;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.linearVelocity = new Vector2(_direction * speed, 0f);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage, transform.position.x);
            Destroy(gameObject);
            return;
        }

        // Ignorar enemigos y al propio boss
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
            return;

        Destroy(gameObject);
    }
}