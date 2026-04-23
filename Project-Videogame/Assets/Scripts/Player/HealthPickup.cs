using UnityEngine;

/// <summary>
/// Pon este script en el prefab del FirstAidKit (botiquín).
/// Al recogerlo recupera vida al jugador.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;   // cuánta vida restaura

    private AudioSource _audio;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.Heal(healAmount);

        if (_audio != null)
            _audio.Play();

        // Oculta el sprite y activa animación de recogida (igual que banana/balas)
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (transform.childCount > 0)
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

        Destroy(gameObject, _audio != null && _audio.clip != null ? _audio.clip.length : 0.3f);
    }
}
