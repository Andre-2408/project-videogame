using UnityEngine;
using System.Collections;

public class EnemyKnifeAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator animator;

    [Header("Movimiento")]
    public float speed = 2f;
    public float stopDistance = 0.5f;

    [Header("Salto")]
    public float jumpForce = 8f;
    public float jumpCooldown = 1.5f;

    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Detección de atasco")]
    public float stuckCheckInterval = 0.4f;
    public float stuckThreshold = 0.05f;

    [Header("Rango de activación")]
    public float activationRange = 8f;

    [Header("Ataque con cuchillo")]
    public float attackCooldown = 1f;       // segundos entre ataques
    public int attackDamage = 1;            // daño por acuchillada
    public float attackRange = 0.8f;        // rango real de daño (puede ser mayor que stopDistance)

    [Header("Sonidos")]
    public AudioClip jumpSound;
    public AudioClip attackSound;

    private Rigidbody2D rb;
    private AudioSource _audio;
    private Transform _visuals;
    private bool _facingRight = true;
    private bool _isGrounded;
    private float _jumpTimer;
    private float _lastPositionX;
    private float _stuckTimer;
    private float _attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

        _lastPositionX = transform.position.x;
        BuildVisualContainer();
    }

    void FixedUpdate()
    {
        float distance = Mathf.Abs(player.position.x - transform.position.x);

        // Si el player está muy lejos, no hacer nada
        if (distance > activationRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isWalking", false);
            animator.SetBool("isShooting", false);
            return;
        }
        // --- Suelo ---
        _isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        // --- Distancia al player ---
        if (distance > stopDistance)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
            animator.SetBool("isWalking", true);
            animator.SetBool("isShooting", false);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isWalking", false);

            // --- Ataque al llegar ---
            _attackTimer -= Time.fixedDeltaTime;
            if (_attackTimer <= 0f)
            {
                Attack();
                _attackTimer = attackCooldown;
            }
        }

        // --- Detección de atasco y salto ---
        _jumpTimer -= Time.fixedDeltaTime;
        _stuckTimer += Time.fixedDeltaTime;

        if (_stuckTimer >= stuckCheckInterval)
        {
            float movedX = Mathf.Abs(transform.position.x - _lastPositionX);
            bool isStuck = movedX < stuckThreshold && distance > stopDistance;

            if (isStuck && _isGrounded && _jumpTimer <= 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                _jumpTimer = jumpCooldown;

                if (_audio != null && jumpSound != null)
                    _audio.PlayOneShot(jumpSound);
            }

            _lastPositionX = transform.position.x;
            _stuckTimer = 0f;
        }

        Flip();
    }

    void Attack()
    {
        // Activa animación
        animator.SetBool("isShooting", true);

        // Sonido
        if (_audio != null && attackSound != null)
            _audio.PlayOneShot(attackSound);

        // Daño real al player si está dentro del rango
        float distance = Mathf.Abs(player.position.x - transform.position.x);
        if (distance <= attackRange)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(attackDamage, transform.position.x);
        }
    }

    void Flip()
    {
        bool lookRight = player.position.x > transform.position.x;
        if (_facingRight == lookRight) return;
        _facingRight = lookRight;
        if (_visuals == null) return;
        Vector3 s = _visuals.localScale;
        s.x = lookRight ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        _visuals.localScale = s;
    }

    private void BuildVisualContainer()
    {
        Transform existing = transform.Find("_Visuals");
        if (existing != null) { _visuals = existing; return; }

        var spriteChildren = new System.Collections.Generic.List<Transform>();
        float sumX = 0f;

        foreach (Transform child in transform)
        {
            if (child.GetComponent<SpriteRenderer>() != null)
            {
                spriteChildren.Add(child);
                sumX += child.localPosition.x;
            }
        }

        if (spriteChildren.Count == 0) { _visuals = transform; return; }

        float centerX = sumX / spriteChildren.Count;
        GameObject container = new GameObject("_Visuals");
        _visuals = container.transform;
        _visuals.SetParent(transform, false);
        _visuals.localPosition = new Vector3(centerX, 0f, 0f);
        _visuals.localScale = Vector3.one;
        _visuals.localRotation = Quaternion.identity;

        foreach (Transform child in spriteChildren)
        {
            Vector3 lp = child.localPosition;
            lp.x -= centerX;
            child.SetParent(_visuals, false);
            child.localPosition = lp;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Muestra el rango de ataque en el editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}