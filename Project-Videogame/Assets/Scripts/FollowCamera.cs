using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector2 offset = new Vector2(0f, 2f);

    void LateUpdate()
    {
        if (player == null) return;

        // Posición deseada (mantenemos el Z fijo para que la cámara no se mueva en profundidad)
        Vector3 desiredPosition = new Vector3(
            player.position.x + offset.x,
            player.position.y + offset.y,
            transform.position.z  // ← Esto es clave en 2D, el Z no debe cambiar
        );

        // Suavizar el movimiento
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}