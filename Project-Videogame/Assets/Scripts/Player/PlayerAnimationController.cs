using UnityEngine;

/// <summary>
/// Sincroniza la animacion de Legs y Torso del jugador.
/// Requiere: un Animator en el root con layers "Legs" y "Torso".
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    // --- Referencias ---
    private Animator _animator;

    // --- Hash de parametros (mas eficiente que strings) ---
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int WeaponType = Animator.StringToHash("weaponType");

    // --- Estado del arma ---
    public enum Weapon { Gun = 0, Knife = 1 }
    private Weapon _currentWeapon = Weapon.Gun;

    // --- Duracion del blend al cambiar arma (segundos) ---
    [SerializeField] private float weaponSwapBlendTime = 0.12f;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // ----------------------------------------------------------------
    // Llamar desde el script de movimiento del jugador
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

    // ----------------------------------------------------------------
    // Cambio de arma fluido
    // Llama esto cuando el jugador cambia de arma.
    // El CrossFade hace que la transicion no se vea cortada.
    // ----------------------------------------------------------------

    public void SwitchWeapon(Weapon newWeapon)
    {
        if (newWeapon == _currentWeapon) return;

        _currentWeapon = newWeapon;
        _animator.SetInteger(WeaponType, (int)newWeapon);

        // Fuerza un crossfade suave en el layer del Torso (layer 1)
        // sin interrumpir el layer de Legs (layer 0)
        string targetState = newWeapon == Weapon.Gun ? "Torso_Idle_Gun" : "Torso_Idle_Knife";
        _animator.CrossFadeInFixedTime(targetState, weaponSwapBlendTime, 1);
    }

    // Acceso al arma actual
    public Weapon CurrentWeapon => _currentWeapon;
}
