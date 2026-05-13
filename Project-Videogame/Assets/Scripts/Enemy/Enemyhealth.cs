using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Salud")]
    public int maxHits = 3;

    [Header("Muerte")]
    [SerializeField] private GameObject deathAnimation;
    [SerializeField] private float deathDelay = 1f;

    [Header("Boss - dejar vacío si no es boss")]
    [Tooltip("Si se asigna, al morir carga esta escena (ideal para el boss final)")]
    [SerializeField] private string winnerScene = "";
    [Tooltip("Puntos que otorga al morir")]
    [SerializeField] private int scoreReward = 0;

    private int _currentHits;
    private HitEffect _hitEffect;
    private bool _isDead = false;

    public int CurrentHits => _currentHits;
    public bool IsDead => _isDead;

    void Start()
    {
        _currentHits = maxHits;
        _hitEffect = GetComponent<HitEffect>();

        if (deathAnimation != null)
            deathAnimation.SetActive(false);
    }

    public void TakeDamage(float attackerX = 0f)
    {
        if (_isDead) return;

        _currentHits--;

        if (_hitEffect != null)
            _hitEffect.TakeHit(attackerX - transform.position.x);

        if (_currentHits <= 0)
            StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        _isDead = true;

        if (scoreReward > 0 && ScoreManager.instance != null)
            ScoreManager.instance.AddPoints(scoreReward);

        var bossAI = GetComponent<GorillaBossAI>();
        if (bossAI != null) bossAI.SetDead();

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;

        if (deathAnimation != null)
        {
            deathAnimation.transform.position = transform.position;
            deathAnimation.SetActive(true);
        }

        yield return new WaitForSeconds(deathDelay);

        if (!string.IsNullOrEmpty(winnerScene))
        {
            if (ScoreManager.instance != null)
                ScoreManager.instance.SaveToPrefs();
            SceneManager.LoadScene(winnerScene);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}