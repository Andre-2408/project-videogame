using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        // Obtenemos el componente AudioSource que ya está en el prefab
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Banana recogida!");

            // Reproducir el sonido
            if (audioSource != null)
            {
                audioSource.Play();
            }

            // Oculta el sprite principal
            GetComponent<SpriteRenderer>().enabled = false;

            // Activa el hijo (la animación de recolección)
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

            // Destruye el objeto después de un tiempo (ajusta según la duración de tu animación)
            Destroy(gameObject, 0.3f);
        }
    }
}
