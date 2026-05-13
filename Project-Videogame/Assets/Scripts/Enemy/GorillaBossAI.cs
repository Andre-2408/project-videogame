using UnityEngine;
using System.Collections;

/// <summary>
/// IA del gorila jefe con 3 fases.
/// 
/// FASE 1 (100-51%): Se acerca mucho y golpea cuerpo a cuerpo
/// FASE 2 (50-26%): Se queda a distancia media, lanza ondas más seguido
/// FASE 3 (25-0%):  Se acerca de nuevo, golpea Y lanza ondas, muy rápido
/// 
/// Si el player está demasiado cerca del boss: empuje + daño
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GorillaBossAnimationController))]
public class GorillaBossAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Velocidad por fase")]
    public float speedPhase1 = 1.5f;
    public float speedPhase2 = 2.5f;
    public float speedPhase3 = 3.8f;

    [Header("Distancia de parada por fase")]
    public float stopDistancePhase1 = 1.2f;  // se acerca mucho
    public float stopDistancePhase2 = 4f;    // se queda lejos para lanzar ondas
    public float stopDistancePhase3 = 1.5f;  // vuelve a acercarse

    [Header("Zona de peligro (demasiado cerca)")]
    public float dangerDistance = 0.6f;      // si el player está aquí -> empuje + daño
    public float knockbackForce = 8f;
    public int contactDamage = 1;
    public float contactCooldown = 1f;       // para no hacer daño cada frame

    [Header("Ataque - Cooldown por fase")]
    public float attackCooldownPhase1 = 2f;
    public float attackCooldownPhase2 = 1.2f;
    public float attackCooldownPhase3 = 0.8f;
    public int punchDamage = 1;
    public float punchRange = 1.8f;

    [Header("Onda de choque")]
    public GameObject shockwavePrefab;
    public Transform shockwaveSpawnPoint;

    [Header("Rugido")]
    public float roarDuration = 1.5f;
    public float roarCooldown = 8f;

    [Header("Sonidos")]
    public AudioClip roarSound;
    public AudioClip punchSound;
    public AudioClip shockwaveSound;
    public AudioClip phase2Sound;
    public AudioClip phase3Sound;
    public AudioClip contactSound;

    [Header("Vida (debe coincidir con EnemyHealth.maxHits)")]
    public int maxHits = 15;

    // ----------------------------------------------------------------
    private Rigidbody2D _rb;
    private GorillaBossAnimationController _anim;
    private EnemyHealth _health;
    private AudioSource _audio;
    private Rigidbody2D _playerRb;

    private int _currentPhase = 1;
    private float _attackTimer = 0f;
    private float _roarTimer = 0f;
    private float _contactTimer = 0f;
    private bool _isRoaring = false;
    private bool _isPunching = false;
    private bool _isDead = false;
    private bool _facingRight = true;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<GorillaBossAnimationController>();
        _health = GetComponent<EnemyHealth>();
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

        if (player != null)
            _playerRb = player.GetComponent<Rigidbody2D>();

        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _roarTimer = roarCooldown * 0.5f;
    }

    void Update()
    {
        if (_isDead || player == null) return;

        CheckPhase();

        float distance = Vector2.Distance(transform.position, player.position);

        // --- Zona de peligro: empuje + daño si player está demasiado cerca ---
        _contactTimer -= Time.deltaTime;
        if (distance < dangerDistance && _contactTimer <= 0f)
        {
            ApplyContactDamage();
            _contactTimer = contactCooldown;
        }

        if (_isRoaring || _isPunching) return;

        // --- Rugido periódico ---
        _roarTimer -= Time.deltaTime;
        if (_roarTimer <= 0f)
        {
            StartCoroutine(Roar());
            _roarTimer = roarCooldown;
            return;
        }

        float stopDist = CurrentStopDistance();
        float speed = CurrentSpeed();

        // --- Movimiento ---
        if (distance > stopDist)
        {
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            _rb.linearVelocity = new Vector2(dir * speed, _rb.linearVelocity.y);
            _anim.SetWalking(true);
            _anim.SetPunching(false);
        }
        else
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            _anim.SetWalking(false);

            // --- Ataque ---
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                StartCoroutine(PunchAttack());
                _attackTimer = CurrentAttackCooldown();
            }
        }

        Flip();
    }

    // ----------------------------------------------------------------
    void CheckPhase()
    {
        if (_health == null) return;
        float pct = (float)_health.CurrentHits / maxHits;

        if (pct > 0.5f && _currentPhase != 1)
        {
            _currentPhase = 1;
        }
        else if (pct <= 0.5f && pct > 0.25f && _currentPhase != 2)
        {
            _currentPhase = 2;
            StartCoroutine(PhaseTransition(phase2Sound));
        }
        else if (pct <= 0.25f && _currentPhase != 3)
        {
            _currentPhase = 3;
            StartCoroutine(PhaseTransition(phase3Sound));
        }
    }

    // ----------------------------------------------------------------
    void ApplyContactDamage()
    {
        // Daño al player
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(contactDamage, transform.position.x);

        // Sonido
        if (_audio != null && contactSound != null)
            _audio.PlayOneShot(contactSound);

        // Empuje: lanza al player en dirección opuesta al boss
        if (_playerRb != null)
        {
            float pushDir = Mathf.Sign(player.position.x - transform.position.x);
            _playerRb.linearVelocity = new Vector2(pushDir * knockbackForce, knockbackForce * 0.5f);
        }
    }

    // ----------------------------------------------------------------
    IEnumerator Roar()
    {
        _isRoaring = true;
        _rb.linearVelocity = Vector2.zero;
        _anim.SetWalking(false);

        if (_audio != null && roarSound != null)
            _audio.PlayOneShot(roarSound);

        yield return new WaitForSeconds(roarDuration);
        _isRoaring = false;
    }

    // ----------------------------------------------------------------
    IEnumerator PunchAttack()
    {
        _isPunching = true;
        _rb.linearVelocity = Vector2.zero;
        _anim.SetPunching(true);

        if (_audio != null && punchSound != null)
            _audio.PlayOneShot(punchSound);

        yield return new WaitForSeconds(0.3f);

        // Daño directo
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= punchRange)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(punchDamage, transform.position.x);

            // Empuje del puñetazo
            if (_playerRb != null)
            {
                float pushDir = Mathf.Sign(player.position.x - transform.position.x);
                _playerRb.linearVelocity = new Vector2(pushDir * knockbackForce, knockbackForce * 0.4f);
            }
        }

        // Fase 2+: onda hacia el frente
        if (_currentPhase >= 2)
            LaunchShockwaves();

        yield return new WaitForSeconds(0.4f);
        _anim.SetPunching(false);
        _isPunching = false;
    }

    void LaunchShockwaves()
    {
        if (shockwavePrefab == null) return;

        Transform spawnPos = shockwaveSpawnPoint != null ? shockwaveSpawnPoint : transform;
        float dir = _facingRight ? 1f : -1f;

        SpawnShockwave(spawnPos.position, dir);

        // Fase 3: también en dirección opuesta
        if (_currentPhase >= 3)
            SpawnShockwave(spawnPos.position, -dir);
    }

    void SpawnShockwave(Vector3 pos, float dir)
    {
        if (_audio != null && shockwaveSound != null)
            _audio.PlayOneShot(shockwaveSound);

        GameObject sw = Instantiate(shockwavePrefab, pos, Quaternion.identity);
        GorillaShockwave shockwave = sw.GetComponent<GorillaShockwave>();
        if (shockwave != null)
        {
            shockwave.damage = _currentPhase >= 3 ? 2 : 1;
            shockwave.SetDirection(dir);
        }
    }

    // ----------------------------------------------------------------
    IEnumerator PhaseTransition(AudioClip sound)
    {
        _isRoaring = true;
        _rb.linearVelocity = Vector2.zero;
        _anim.SetWalking(false);

        if (_audio != null && sound != null)
            _audio.PlayOneShot(sound);

        yield return new WaitForSeconds(1.5f);
        _isRoaring = false;
    }

    // ----------------------------------------------------------------
    float CurrentSpeed() => _currentPhase switch
    {
        2 => speedPhase2,
        3 => speedPhase3,
        _ => speedPhase1
    };

    float CurrentStopDistance() => _currentPhase switch
    {
        2 => stopDistancePhase2,
        3 => stopDistancePhase3,
        _ => stopDistancePhase1
    };

    float CurrentAttackCooldown() => _currentPhase switch
    {
        2 => attackCooldownPhase2,
        3 => attackCooldownPhase3,
        _ => attackCooldownPhase1
    };

    void Flip()
    {
        bool lookRight = player.position.x > transform.position.x;
        if (_facingRight == lookRight) return;
        _facingRight = lookRight;
        Vector3 s = transform.localScale;
        s.x = lookRight ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    public void SetDead() => _isDead = true;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, punchRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, dangerDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistancePhase2);
    }
}