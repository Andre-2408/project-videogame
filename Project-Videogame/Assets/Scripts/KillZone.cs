using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Coloca este script en un GameObject con Collider2D (Is Trigger = true)
/// debajo del mapa. Cuando el jugador cae, recibe daño y reaparece en spawnPoint.
/// Si muere por la caída, la escena se recarga normalmente (PlayerHealth lo maneja).
/// </summary>
public class KillZone : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;  // arrastrar el SpawnPoint del jugador aquí
    [SerializeField] private int       damage = 1;  // daño por caer al vacío
    [Header("Nombre de la escena a cargar")]
    [Tooltip("Debe coincidir con el nombre en Build Settings")]
    public string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        var health = col.GetComponent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(damage);

        // Solo reaparece si sobrevivió la caída
        if (health.IsAlive)
        {
            col.transform.position = spawnPoint.position;

            // Resetear velocidad para que no siga con el impulso de la caída
            var rb = col.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        else
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("No se ha asignado ninguna escena en el Inspector.");
            }
        }
    }
}
