using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(PlayerAmmo))]
[RequireComponent(typeof(PlayerShoot))]
[RequireComponent(typeof(PowerUpManager))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Deteccion de suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float     groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("RapidFire")]
    [SerializeField] private float rapidFireInterval = 0.12f;

    private Rigidbody2D              _rb;
    private PlayerAnimationController _anim;
    private PlayerAmmo               _ammo;
    private PlayerShoot              _shoot;
    private PowerUpManager           _powerUp;

    private bool  _isGrounded;
    private bool  _wasGrounded;
    private bool  _wasWalking;
    private bool  _facingRight = true;
    private float _moveX;

    private bool  _inputReady = false;
    private const float InputDelay = 0.1f;

    private float _rapidFireTimer = 0f;

    // ════════════════════════════════════════════
    void Awake()
    {
        _rb      = GetComponent<Rigidbody2D>();
        _anim    = GetComponent<PlayerAnimationController>();
        _ammo    = GetComponent<PlayerAmmo>();
        _shoot   = GetComponent<PlayerShoot>();
        _powerUp = GetComponent<PowerUpManager>();
    }

    void Start() => Invoke(nameof(EnableInput), InputDelay);
    void EnableInput() => _inputReady = true;

    // ════════════════════════════════════════════
    void Update()
    {
        if (!_inputReady) return;

        var keyboard = Keyboard.current;
        var mouse    = Mouse.current;
        if (keyboard == null) return;

        // ── Movimiento horizontal ─────────────────
        _moveX = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  _moveX = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _moveX =  1f;

        bool isWalking = _moveX != 0;
        if (isWalking != _wasWalking) { _anim.SetWalking(isWalking); _wasWalking = isWalking; }

        // ── Flip ──────────────────────────────────
        if (_moveX > 0 && !_facingRight) SetFacing(true);
        if (_moveX < 0 &&  _facingRight) SetFacing(false);

        // ── Suelo ─────────────────────────────────
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (_isGrounded != _wasGrounded) { _anim.SetJumping(!_isGrounded); _wasGrounded = _isGrounded; }

        // ── Salto ─────────────────────────────────
        if (keyboard.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _anim.PlayJumpSound();
        }

        // ── Disparo ───────────────────────────────
        _rapidFireTimer -= Time.deltaTime;

        if (mouse != null)
        {
            bool isRapidFire = _powerUp != null && _powerUp.ActiveType == PowerUpType.RapidFire;

            bool shootNow = isRapidFire
                ? mouse.leftButton.isPressed && _rapidFireTimer <= 0f
                : mouse.leftButton.wasPressedThisFrame;

            if (shootNow)
            {
                bool isReloading = _ammo != null && _ammo.IsReloading;
                bool hasAmmo     = _ammo != null && _ammo.Magazine > 0;

                if (!isReloading)
                {
                    _anim.TriggerAttack();
                    if (_anim.CurrentWeapon == PlayerAnimationController.Weapon.Gun && hasAmmo)
                    {
                        _shoot.Shoot();
                        if (isRapidFire) _rapidFireTimer = rapidFireInterval;
                    }
                }
            }
        }

        // ── Recarga R ─────────────────────────────
        if (keyboard.rKey.wasPressedThisFrame &&
            _anim.CurrentWeapon == PlayerAnimationController.Weapon.Gun &&
            _ammo != null)
        {
            if (_ammo.TryReload(() => { })) _anim.PlayReloadSound();
        }

        // ── Cambio de arma E ──────────────────────
        if (keyboard.eKey.wasPressedThisFrame)
        {
            var next = _anim.CurrentWeapon == PlayerAnimationController.Weapon.Gun
                ? PlayerAnimationController.Weapon.Knife
                : PlayerAnimationController.Weapon.Gun;
            _anim.SwitchWeapon(next);
        }
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_moveX * moveSpeed, _rb.linearVelocity.y);
    }

    private void SetFacing(bool right)
    {
        _facingRight = right;
        _anim.SetFacingRight(right);
        _shoot.SetFacing(right);
    }
}
