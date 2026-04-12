using UnityEngine;

public class EnemyAI : MonoBehaviour
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

    private Rigidbody2D rb;
    private Transform _visuals;
    private bool _facingRight = true;
    private bool _isGrounded;
    private float _jumpTimer;
    private float _lastPositionX;
    private float _stuckTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        _lastPositionX = transform.position.x;
        BuildVisualContainer();
    }

    void FixedUpdate()
    {
        // --- Suelo ---
        _isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.05f;
        animator.SetBool("isJumping", !_isGrounded);

        // --- Movimiento horizontal ---
        float distance = Mathf.Abs(player.position.x - transform.position.x);

        if (distance > stopDistance)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
            animator.SetBool("isWalking", true);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isWalking", false);
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
            }

            _lastPositionX = transform.position.x;
            _stuckTimer = 0f;
        }

        Flip();
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
    }
}