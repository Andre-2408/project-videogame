using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Flip: en Awake() agrupa todos los SpriteRenderer hijos en un contenedor "_Visuals"
/// centrado en el centro visual real del personaje. Al voltear solo se escala ese
/// contenedor en X -> no hay teletransporte, no se desalinean los sprites.
///
/// Swap: SetActive entre gunTorso y knifeTorso (busca por nombre que contenga
/// "Weapon" o "Knife", o asignar en Inspector).
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Sonidos de ataque")]
    [SerializeField] private AudioClip gunSound;
    [SerializeField] private AudioClip knifeSound;
    [SerializeField] private AudioClip emptyGunSound;  // sonido pistola sin balas
    [SerializeField] private AudioClip reloadSound;    // sonido al recargar

    [Header("Sonidos de movimiento y daño")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip damageSound;

    private AudioSource _audio;
    private Animator _animator;
    private PlayerAmmo _ammo;
    private static readonly int IsWalking   = Animator.StringToHash("isWalking");
    private static readonly int IsJumping   = Animator.StringToHash("isJumping");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int WeaponType  = Animator.StringToHash("weaponType");

    // Contenedor visual — unico objeto que se flippea en X
    private Transform _visuals;

    public enum Weapon { Gun = 0, Knife = 1 }
    private Weapon _currentWeapon = Weapon.Gun;

    // ----------------------------------------------------------------
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _audio    = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _ammo     = GetComponent<PlayerAmmo>();

        BuildVisualContainer();   // agrupa sprites y calcula centro real
    }

    // ----------------------------------------------------------------
    // Construye el contenedor visual centrado en el promedio de los sprites
    // ----------------------------------------------------------------
    private void BuildVisualContainer()
    {
        // Recoger hijos directos con SpriteRenderer
        var spriteChildren = new List<Transform>();
        float sumX = 0f;

        foreach (Transform child in transform)
        {
            if (child.GetComponent<SpriteRenderer>() != null)
            {
                spriteChildren.Add(child);
                sumX += child.localPosition.x;
            }
        }

        if (spriteChildren.Count == 0)
        {
            _visuals = transform;
            Debug.LogWarning("[PlayerAnim] No se encontraron SpriteRenderers hijos directos.");
            return;
        }

        // Centro visual en X
        float centerX = sumX / spriteChildren.Count;

        // Crear (o reusar) contenedor
        Transform existing = transform.Find("_Visuals");
        GameObject container = existing != null
            ? existing.gameObject
            : new GameObject("_Visuals");

        _visuals = container.transform;
        _visuals.SetParent(transform, false);
        _visuals.localPosition = new Vector3(centerX, 0f, 0f);
        _visuals.localScale    = Vector3.one;
        _visuals.localRotation = Quaternion.identity;

        // Mover sprites al contenedor ajustando su posicion relativa
        foreach (Transform child in spriteChildren)
        {
            Vector3 lp = child.localPosition;
            lp.x -= centerX;
            child.SetParent(_visuals, false);
            child.localPosition = lp;
        }

    }

    // ----------------------------------------------------------------
    // Animacion
    // ----------------------------------------------------------------
    public void SetWalking(bool walking)
    {
        if (_animator != null) _animator.SetBool(IsWalking, walking);
    }

    public void SetJumping(bool jumping)
    {
        if (_animator != null) _animator.SetBool(IsJumping, jumping);
    }

    public void TriggerAttack()
    {
        if (_currentWeapon == Weapon.Gun)
        {
            // Con pistola: verificar balas
            bool hasAmmo = _ammo == null || _ammo.TrySpend();
            if (!hasAmmo)
            {
                // Sin balas: solo sonido de vacio, sin animacion de ataque
                if (_audio != null && emptyGunSound != null)
                    _audio.PlayOneShot(emptyGunSound);
                return;
            }
            if (_audio != null && gunSound != null)
                _audio.PlayOneShot(gunSound);
        }
        else
        {
            // Cuchillo: no consume balas
            if (_audio != null && knifeSound != null)
                _audio.PlayOneShot(knifeSound);
        }

        if (_animator != null) _animator.SetTrigger(IsAttacking);
    }

    // ----------------------------------------------------------------
    // Swap de arma
    // El Animator tiene estados Knife/Gun pero sin transiciones por weaponType,
    // por eso forzamos Play() en el layer 1 (Torso) al estado Idle del arma nueva.
    // Las transiciones normales (isWalking/isAttacking) se encargan del resto.
    // ----------------------------------------------------------------
    public void SwitchWeapon(Weapon newWeapon)
    {
        if (newWeapon == _currentWeapon) return;
        _currentWeapon = newWeapon;

        if (_animator != null)
        {
            _animator.SetInteger(WeaponType, (int)newWeapon);
            string idleState = newWeapon == Weapon.Gun ? "Torso_Idle_Gun" : "Torso_Idle_Knife";
            _animator.Play(idleState, 1, 0f); // layer 1 = Torso, desde el inicio
        }

        Debug.Log($"[WeaponSwap] {_currentWeapon}");
    }

    // ----------------------------------------------------------------
    // Flip — escala solo _Visuals, el root/RB/Collider no se mueven
    // ----------------------------------------------------------------
    public void SetFacingRight(bool facingRight)
    {
        if (_visuals == null) return;
        Vector3 s = _visuals.localScale;
        s.x = facingRight ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        _visuals.localScale = s;
    }

    public void PlayJumpSound()
    {
        if (_audio != null && jumpSound != null)
            _audio.PlayOneShot(jumpSound);
    }

    public void PlayDamageSound()
    {
        if (_audio != null && damageSound != null)
            _audio.PlayOneShot(damageSound);
    }

    public void PlayReloadSound()
    {
        if (_audio != null && reloadSound != null)
            _audio.PlayOneShot(reloadSound);
    }

    public Weapon CurrentWeapon => _currentWeapon;
}
