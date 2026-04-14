using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;

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
        // Daña al player
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(1);

            Destroy(gameObject);
            return;
        }

        // Se destruye con cualquier otra cosa excepto el enemigo
        if (!other.CompareTag("Enemy"))
            Destroy(gameObject);
    }
}