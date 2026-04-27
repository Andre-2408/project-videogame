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
    public float jumpCooldown = 1.2f;

    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Detección de pared / borde")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.4f;
    public Transform edgeCheck;
    public float edgeCheckDistance = 0.5f;

    [Header("Detección de atasco (respaldo)")]
    public float stuckCheckInterval = 0.5f;
    public float stuckThreshold = 0.05f;

    [Header("Rango de activación")]
    public float activationRange = 8f;

    [Header("Ataque con cuchillo")]
    public float attackCooldown = 1f;
    public int   attackDamage   = 1;
    public float attackRange    = 0.8f;

    [Header("Sonidos")]
    public AudioClip jumpSound;
    public AudioClip attackSound;

    // ── privados ──────────────────────────────────────
    private Rigidbody2D _rb;
    private AudioSource _audio;
    private Transform   _visuals;
    private bool        _facingRight = true;
    private bool        _isGrounded;
    private float       _jumpTimer;
    private float       _lastPositionX;
    private float       _stuckTimer;
    private float       _attackTimer;

    // ════════════════════════════════════════════════
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

        _lastPositionX = transform.position.x;
        BuildVisualContainer();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // ── Detección de suelo real (con fallback a velocidad) ──
        bool physicsGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        _isGrounded = physicsGrounded || Mathf.Abs(_rb.linearVelocity.y) < 0.08f;

        float distX = Mathf.Abs(player.position.x - transform.position.x);

        if (distX > activationRange)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            SetAnim(false, false);
            return;
        }

        float dir = Mathf.Sign(player.position.x - transform.position.x);

        if (distX > stopDistance)
        {
            // ── Detectar borde delante ───────────────
            bool voidAhead = false;
            if (edgeCheck != null)
            {
                Vector2 edgeOrigin = new Vector2(
                    transform.position.x + dir * edgeCheckDistance,
                    edgeCheck.position.y);
                voidAhead = !Physics2D.Raycast(edgeOrigin, Vector2.down, 0.6f, groundLayer);
            }

            // ── Detectar pared delante (sin filtro de layer, detecta cualquier obstáculo) ──
            bool wallAhead = false;
            if (wallCheck != null)
            {
                // Excluye al propio enemigo y al player para no dispararse a sí mismo
                int mask = ~(LayerMask.GetMask("Enemy") | LayerMask.GetMask("Player"));
                wallAhead = Physics2D.Raycast(wallCheck.position, new Vector2(dir, 0),
                                              wallCheckDistance, mask);
            }

            if (voidAhead && _isGrounded)
            {
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                SetAnim(false, false);
            }
            else
            {
                _rb.linearVelocity = new Vector2(dir * speed, _rb.linearVelocity.y);
                SetAnim(true, false);

                if (wallAhead && _isGrounded && _jumpTimer <= 0f)
                    DoJump();
            }
        }
        else
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            SetAnim(false, false);

            if (_isGrounded)
            {
                _attackTimer -= Time.fixedDeltaTime;
                if (_attackTimer <= 0f)
                {
                    Attack();
                    _attackTimer = attackCooldown;
                }
            }
        }

        // ── Anti-atasco (respaldo) ───────────────────
        _jumpTimer  -= Time.fixedDeltaTime;
        _stuckTimer += Time.fixedDeltaTime;

        if (_stuckTimer >= stuckCheckInterval)
        {
            float movedX = Mathf.Abs(transform.position.x - _lastPositionX);
            bool  isStuck = movedX < stuckThreshold && distX > stopDistance;

            if (isStuck && _isGrounded && _jumpTimer <= 0f)
                DoJump();

            _lastPositionX = transform.position.x;
            _stuckTimer    = 0f;
        }

        Flip(dir);
    }

    // ── Helpers ──────────────────────────────────────
    void DoJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        _jumpTimer = jumpCooldown;
        if (_audio != null && jumpSound != null)
            _audio.PlayOneShot(jumpSound);
    }

    void Attack()
    {
        SetAnim(false, true);

        if (_audio != null && attackSound != null)
            _audio.PlayOneShot(attackSound);

        float dist = player != null
            ? Mathf.Abs(player.position.x - transform.position.x)
            : float.MaxValue;

        if (dist <= attackRange)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(attackDamage, transform.position.x);
        }
    }

    void SetAnim(bool walking, bool attacking)
    {
        if (animator == null) return;
        animator.SetBool("isWalking",  walking);
        animator.SetBool("isShooting", attacking);
    }

    void Flip(float dir)
    {
        bool lookRight = dir > 0;
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
        _visuals.localScale    = Vector3.one;
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
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(wallCheck.position, Vector3.right * wallCheckDistance);
        }
        if (edgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(edgeCheck.position, Vector3.down * 0.6f);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
