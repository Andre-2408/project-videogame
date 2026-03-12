using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Banana recogida!");

            // Oculta el sprite principal
            GetComponent<SpriteRenderer>().enabled = false;

            // Activa el hijo (la animación de recolección)
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

            // Destruye el objeto después de un tiempo (ajusta según la duración de tu animación)
            Destroy(gameObject, 0.3f);
        }
    }
}
