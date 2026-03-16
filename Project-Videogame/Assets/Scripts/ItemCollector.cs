using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    private AudioSource audioSource;
    public int points = 10;
    void Start()
    {
        // Obtenemos el componente AudioSource que ya est� en el prefab
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("�Banana recogida!");

            // Reproducir el sonido
            if (audioSource != null)
            {
                audioSource.Play();
            }

            // Oculta el sprite principal
            GetComponent<SpriteRenderer>().enabled = false;

            // Activa el hijo (la animaci�n de recolecci�n)
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

            FindFirstObjectByType<ScoreManager>().AddPoints(points);

            // Destruye el objeto despu�s de un tiempo (ajusta seg�n la duraci�n de tu animaci�n)
            Destroy(gameObject, 0.3f);
        }
    }
}
