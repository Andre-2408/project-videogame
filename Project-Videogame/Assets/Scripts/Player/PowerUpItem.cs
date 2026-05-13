using UnityEngine;

/// <summary>
/// Item de power-up que recoge el jugador.
///
/// SETUP en Unity Editor:
///   1. Crea un prefab con SpriteRenderer + CircleCollider2D (Is Trigger = true)
///   2. Añade este script
///   3. Asigna el tipo de power-up en "powerUpType"
///   4. Opcionalmente añade AudioSource para sonido al recoger
/// </summary>
public class PowerUpItem : MonoBehaviour
{
    [Header("Power-up")]
    public PowerUpType powerUpType = PowerUpType.SpreadShot;
    public float duration = 10f;

    private AudioSource _audio;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var powerUp = other.GetComponent<PowerUpManager>();
        if (powerUp != null)
            powerUp.Activate(powerUpType, duration);

        if (_audio != null)
            _audio.Play();

        // Ocultar sprite y destruir (mismo patrón que HealthPickup)
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (transform.childCount > 0)
        {
            var childSr = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (childSr != null) childSr.enabled = true;
        }

        Destroy(gameObject, _audio != null && _audio.clip != null ? _audio.clip.length : 0.3f);
    }
}
