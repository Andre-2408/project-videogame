using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(PlayerAmmo))]
[RequireComponent(typeof(PlayerShoot))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Deteccion de suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D _rb;
    private PlayerAnimationController _anim;
    private PlayerAmmo _ammo;
    private PlayerShoot _shoot;

    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _wasWalking;
    private bool _facingRight = true;
    private float _moveX;

    private bool _inputReady = false;
    private const float InputDelay = 0.1f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<PlayerAnimationController>();
        _ammo = GetComponent<PlayerAmmo>();
        _shoot = GetComponent<PlayerShoot>();
    }

    void Start()
    {
        Invoke(nameof(EnableInput), InputDelay);
    }

    void EnableInput() => _inputReady = true;

    void Update()
    {
        if (!_inputReady) return;

        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null) return;

        // --- Movimiento horizontal ---
        _moveX = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) _moveX = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _moveX = 1f;

        bool isWalking = _moveX != 0;
        if (isWalking != _wasWalking)
        {
            _anim.SetWalking(isWalking);
            _wasWalking = isWalking;
        }

        // --- Flip ---
        if (_moveX > 0 && !_facingRight) SetFacing(true);
        if (_moveX < 0 && _facingRight) SetFacing(false);

        // --- Deteccion de suelo (solo actualiza Animator si cambia el estado) ---
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (_isGrounded != _wasGrounded)
        {
            _anim.SetJumping(!_isGrounded);
            _wasGrounded = _isGrounded;
        }

        // --- Salto ---
        if (keyboard.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _anim.PlayJumpSound();
        }

        // --- Ataque / Disparo ---
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            bool isReloading = _ammo != null && _ammo.IsReloading;
            bool hasAmmo = _ammo != null && _ammo.Magazine > 0;

            if (!isReloading)
            {
                _anim.TriggerAttack(); // esto ya maneja el sonido de vacio

                // Solo instancia la bala si tiene balas
                if (_anim.CurrentWeapon == PlayerAnimationController.Weapon.Gun && hasAmmo)
                    _shoot.Shoot();
            }
        }

        // --- Recarga (R) ---
        if (keyboard.rKey.wasPressedThisFrame &&
            _anim.CurrentWeapon == PlayerAnimationController.Weapon.Gun &&
            _ammo != null)
        {
            bool started = _ammo.TryReload(() => { });
            if (started) _anim.PlayReloadSound();
        }

        // --- Cambio de arma con E ---
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
        _shoot.SetFacing(right); // sincroniza dirección de disparo
    }
}