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
    [SerializeField] private Image healthBarFill;

    [Header("Muerte")]
    [SerializeField] private float deathDelay = 0.5f;
    [SerializeField] private string deathScene;
    [SerializeField] private GameObject deathAnimation; // arrastra MonkeyDie_0 aquí

    private PlayerAnimationController _anim;
    private HitEffect _hitEffect;

    void Awake()
    {
        _anim = GetComponent<PlayerAnimationController>();
        _hitEffect = GetComponent<HitEffect>();
        _currentHealth = maxHealth;
        UpdateBar();

        if (deathAnimation != null)
            deathAnimation.SetActive(false);
    }

    public bool IsAlive => _currentHealth > 0;

    public void TakeDamage(int amount = 1, float attackerX = 0f)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= amount;
        _currentHealth = Mathf.Max(0, _currentHealth);

        if (_anim != null) _anim.PlayDamageSound();
        if (_hitEffect != null) _hitEffect.TakeHit(attackerX - transform.position.x);

        UpdateBar();

        if (_currentHealth <= 0)
            StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        // Bloquear movimiento y input
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static; // congela el rigidbody
        }

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false; // desactiva el script de movimiento

        // Ocultar sprites normales del player
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;

        // Activar animación de muerte en la misma posición
        if (deathAnimation != null)
        {
            deathAnimation.transform.position = transform.position;
            deathAnimation.SetActive(true);
        }

        yield return new WaitForSeconds(deathDelay);

        if (!string.IsNullOrEmpty(deathScene))
            SceneManager.LoadScene(deathScene);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)_currentHealth / maxHealth;
    }
}