using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    private int _currentHealth;

    [Header("UI")]
    [SerializeField] private Image healthBarFill;   // Image tipo Filled, Fill Method = Horizontal

    // Referencia al audio del player para el sonido de daño
    private PlayerAnimationController _anim;

    [Header("Tiempo antes de recargar escena al morir")]
    [SerializeField] private float deathDelay = 0.5f;

    void Awake()
    {
        _anim = GetComponent<PlayerAnimationController>();
        _currentHealth = maxHealth;
        UpdateBar();
    }

    public bool IsAlive => _currentHealth > 0;

    public void TakeDamage(int amount = 1)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= amount;
        _currentHealth = Mathf.Max(0, _currentHealth);

        if (_anim != null) _anim.PlayDamageSound();

        UpdateBar();

        if (_currentHealth <= 0)
            StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        // Desactivar visualmente para que el sonido suene primero
        gameObject.SetActive(false);
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)_currentHealth / maxHealth;
    }
}
