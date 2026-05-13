using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed    = 12f;
    public float lifetime = 5f;

    [Header("Zigzag (solo aplica en modo Zigzag)")]
    public float zigzagAmplitude  = 4f;
    public float zigzagFrequency  = 6f;

    public enum BulletMode { Normal, Zigzag }
    public BulletMode mode = BulletMode.Normal;

    private Rigidbody2D _rb;
    private float       _direction = 1f;
    private float       _timer;
    private float       _startTime;

    // ── Dirección simple (normal y spread) ───────
    public void SetDirection(float dir)
    {
        _direction = dir;
        _rb = _rb != null ? _rb : GetComponent<Rigidbody2D>();
        if (_rb != null)
            _rb.linearVelocity = new Vector2(_direction * speed, 0f);
    }

    // ── Velocidad arbitraria (spread con ángulo) ─
    public void SetVelocity(Vector2 vel)
    {
        _direction = Mathf.Sign(vel.x);
        _rb = _rb != null ? _rb : GetComponent<Rigidbody2D>();
        if (_rb != null)
            _rb.linearVelocity = vel;
    }

    // ════════════════════════════════════════════
    void OnEnable()
    {
        _rb = _rb != null ? _rb : GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            _rb.gravityScale   = 0f;
            _rb.linearVelocity = new Vector2(_direction * speed, 0f);
        }
        _timer     = lifetime;
        _startTime = Time.time;
        mode       = BulletMode.Normal;   // reset al volver al pool
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f) { Recycle(); return; }

        if (mode == BulletMode.Zigzag && _rb != null)
        {
            float yVel = Mathf.Sin((Time.time - _startTime) * zigzagFrequency) * zigzagAmplitude;
            _rb.linearVelocity = new Vector2(_direction * speed, yVel);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(transform.position.x);
            Recycle();
            return;
        }
        if (!other.CompareTag("Player"))
            Recycle();
    }

    private void Recycle()
    {
        if (BulletPool.Instance != null)
            BulletPool.Instance.ReturnPlayerBullet(gameObject);
        else
            Destroy(gameObject);
    }
}
