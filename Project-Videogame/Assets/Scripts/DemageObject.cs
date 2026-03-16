using UnityEngine;

public class DemageObject : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Player")) return;

        var health = collision.gameObject.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}
