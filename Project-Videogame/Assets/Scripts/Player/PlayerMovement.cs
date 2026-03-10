using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Deteccion de suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Sprites para flip directo")]
    [SerializeField] private SpriteRenderer legsRenderer;
    [SerializeField] private SpriteRenderer torsoRenderer;

    private Rigidbody2D _rb;
    private PlayerAnimationController _anim;

    private bool _isGrounded;
    private bool _facingRight = true;
    private float _moveX;

    // Evita que el primer click al enfocar la Game View dispare el ataque
    private bool _inputReady = false;
    private float _inputDelay = 0.1f;

    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _anim = GetComponent<PlayerAnimationController>();
    }

    void Start()
    {
        // Espera un momento antes de aceptar input para evitar
        // que el click de enfoque de la Game View dispare el ataque
        Invoke(nameof(EnableInput), _inputDelay);
    }

    void EnableInput() => _inputReady = true;

    void Update()
    {
        if (!_inputReady) return;

        var keyboard = Keyboard.current;
        var mouse    = Mouse.current;

        if (keyboard == null) return;

        // --- Movimiento horizontal ---
        _moveX = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  _moveX = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _moveX =  1f;

        _anim.SetWalking(_moveX != 0);

        // --- Flip directo en SpriteRenderers ---
        if (_moveX > 0 && !_facingRight) SetFacing(true);
        if (_moveX < 0 &&  _facingRight) SetFacing(false);

        // --- Deteccion de suelo ---
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        _anim.SetJumping(!_isGrounded);

        // --- Salto ---
        if (keyboard.spaceKey.wasPressedThisFrame && _isGrounded)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);

        // --- Ataque ---
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            _anim.TriggerAttack();

        // --- Cambio de arma ---
        if (keyboard.eKey.wasPressedThisFrame)
        {
            var newWeapon = _anim.CurrentWeapon == PlayerAnimationController.Weapon.Gun
                ? PlayerAnimationController.Weapon.Knife
                : PlayerAnimationController.Weapon.Gun;
            _anim.SwitchWeapon(newWeapon);
        }
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_moveX * moveSpeed, _rb.linearVelocity.y);
    }

    private void SetFacing(bool right)
    {
        _facingRight = right;
        bool flip = !right;
        if (legsRenderer  != null) legsRenderer.flipX  = flip;
        if (torsoRenderer != null) torsoRenderer.flipX = flip;
    }
}
