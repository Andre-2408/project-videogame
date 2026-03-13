using UnityEngine;

public class DemageObject : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Debug.Log("Player Demaged");
            Destroy(collision.gameObject);
        }
    }
}
