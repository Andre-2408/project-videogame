using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 3f; // se destruye sola si no golpea nada

    private float _direction = 1f;

    public void SetDirection(float dir)
    {
        _direction = dir;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * _direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Golpea enemigo
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage();
            Destroy(gameObject);
            return;
        }

        // Golpea suelo u otros obstáculos (ignora al player)
        if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}