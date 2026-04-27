using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed    = 10f;
    public float lifetime = 5f;

    private Rigidbody2D _rb;
    private float       _direction = 1f;
    private float       _timer;

    public void SetDirection(float dir)
    {
        _direction = dir;
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
            _rb.linearVelocity = new Vector2(_direction * speed, 0f);
    }

    void OnEnable()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            _rb.gravityScale   = 0f;
            _rb.linearVelocity = new Vector2(_direction * speed, 0f);
        }
        _timer = lifetime;
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f) Recycle();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1);
            Recycle();
            return;
        }
        if (!other.CompareTag("Enemy"))
            Recycle();
    }

    private void Recycle()
    {
        if (BulletPool.Instance != null)
            BulletPool.Instance.ReturnEnemyBullet(gameObject);
        else
            Destroy(gameObject);
    }
}
