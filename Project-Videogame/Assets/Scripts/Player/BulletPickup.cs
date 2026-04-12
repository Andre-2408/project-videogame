using UnityEngine;

/// <summary>
/// Pon este script en el prefab de Bullets.
/// Al recogerlo agrega balas parciales al jugador.
/// </summary>
public class BulletPickup : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 8;   // balas que da (no llena completo)

    private AudioSource _audio;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var ammo = other.GetComponent<PlayerAmmo>();
        if (ammo != null)
            ammo.AddAmmo(ammoAmount);

        if (_audio != null)
            _audio.Play();

        GetComponent<SpriteRenderer>().enabled = false;
        // Activa el hijo (la animaci?n de recolecci?n)
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        Destroy(gameObject, _audio != null ? _audio.clip.length : 0f);
    }
}
