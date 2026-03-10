using UnityEngine;

/// <summary>
/// Controla la animacion del enemigo.
/// El tipo de arma se asigna al instanciar (Knife o Gun) y no cambia.
/// Las piernas son compartidas entre todos los tipos.
/// </summary>
public class EnemyAnimationController : MonoBehaviour
{
    public enum EnemyWeapon { Gun = 0, Knife = 1 }

    [Header("Tipo de arma de este enemigo")]
    [SerializeField] private EnemyWeapon weaponType = EnemyWeapon.Gun;

    private Animator _animator;

    private static readonly int IsWalking   = Animator.StringToHash("isWalking");
    private static readonly int IsJumping   = Animator.StringToHash("isJumping");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int WeaponType  = Animator.StringToHash("weaponType");
    private static readonly int IsDead      = Animator.StringToHash("isDead");

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Aplica el tipo de arma al nacer -- sin ninguna transicion visible
        // porque aun no ha empezado a moverse
        _animator.SetInteger(WeaponType, (int)weaponType);
    }

    // ----------------------------------------------------------------
    // Llamar desde el script de IA del enemigo
    // ----------------------------------------------------------------

    public void SetWalking(bool walking)
    {
        _animator.SetBool(IsWalking, walking);
    }

    public void SetJumping(bool jumping)
    {
        _animator.SetBool(IsJumping, jumping);
    }

    public void TriggerAttack()
    {
        _animator.SetTrigger(IsAttacking);
    }

    public void TriggerDeath()
    {
        _animator.SetBool(IsDead, true);
    }

    // Permite sobreescribir el tipo de arma desde el spawner antes de Start()
    public void Initialize(EnemyWeapon weapon)
    {
        weaponType = weapon;
    }
}
