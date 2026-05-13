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

    [Header("Vidas y Muerte")]
    [SerializeField] private float deathDelay   = 0.5f;
    [SerializeField] private int   maxLives      = 3;
    [SerializeField] private string gameOverScene = "GameOverScene";
    [SerializeField] private GameObject deathAnimation;

    // ── Acceso ───────────────────────────────────────
    public bool IsAlive        => _currentHealth > 0;
    public int  CurrentHealth  => _currentHealth;
    public int  MaxHealth      => maxHealth;

    // Evento para HealthBarUI
    public event System.Action<int, int> OnHealthChanged;

    // Evento estático para LivesHUD (persiste entre escenas)
    public static event System.Action<int> OnLivesChanged;

    // ── PlayerPrefs keys ─────────────────────────────
    public const string LivesKey     = "PlayerLives";
    public const string LastSceneKey = "LastScene";

    // ── Llamar desde Menu al iniciar partida ─────────
    public static void InitNewGame(int lives = 3)
    {
        PlayerPrefs.SetInt(LivesKey, lives);
        PlayerPrefs.SetString(LastSceneKey, "SampleScene");
        PlayerPrefs.Save();
    }

    public static int GetCurrentLives() => PlayerPrefs.GetInt(LivesKey, 3);

    // ── Añadir vida extra (item) ─────────────────────
    public void AddLife(int amount = 1)
    {
        int lives = Mathf.Min(PlayerPrefs.GetInt(LivesKey, maxLives) + amount, maxLives);
        PlayerPrefs.SetInt(LivesKey, lives);
        PlayerPrefs.Save();
        OnLivesChanged?.Invoke(lives);
    }

    // ════════════════════════════════════════════════
    void Awake()
    {
        _anim      = GetComponent<PlayerAnimationController>();
        _hitEffect = GetComponent<HitEffect>();
        _currentHealth = maxHealth;
        UpdateBar();

        if (deathAnimation != null)
            deathAnimation.SetActive(false);

        // Guardar escena actual como último checkpoint
        PlayerPrefs.SetString(LastSceneKey, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    private PlayerAnimationController _anim;
    private HitEffect _hitEffect;

    // ════════════════════════════════════════════════
    public void Heal(int amount = 1)
    {
        if (_currentHealth <= 0) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        UpdateBar();
    }

    public void TakeDamage(int amount = 1, float attackerX = 0f)
    {
        if (_currentHealth <= 0) return;
        _currentHealth -= amount;
        _currentHealth  = Mathf.Max(0, _currentHealth);

        if (_anim      != null) _anim.PlayDamageSound();
        if (_hitEffect != null) _hitEffect.TakeHit(attackerX - transform.position.x);

        UpdateBar();

        if (_currentHealth <= 0)
            StartCoroutine(Die());
    }

    // ════════════════════════════════════════════════
    private IEnumerator Die()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Static; }

        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;

        if (deathAnimation != null)
        {
            deathAnimation.transform.position = transform.position;
            deathAnimation.SetActive(true);
        }

        yield return new WaitForSeconds(deathDelay);

        // ── Sistema de vidas ──────────────────────
        int lives = Mathf.Max(0, PlayerPrefs.GetInt(LivesKey, maxLives) - 1);
        PlayerPrefs.SetInt(LivesKey, lives);
        PlayerPrefs.Save();
        OnLivesChanged?.Invoke(lives);

        if (lives <= 0)
        {
            if (ScoreManager.instance != null)
                ScoreManager.instance.SaveToPrefs();
            SceneManager.LoadScene(gameOverScene);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)_currentHealth / maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }
}
