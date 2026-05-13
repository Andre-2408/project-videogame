using UnityEngine;

/// <summary>
/// Controlador de animación del gorila jefe.
/// Parámetros necesarios en el Animator:
/// - IsWalking (Bool)
/// - IsPunch (Bool)
/// </summary>
public class GorillaBossAnimationController : MonoBehaviour
{
    private Animator _animator;
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsPunch = Animator.StringToHash("IsPunch");

    // Sprites del gorila para el flash de daño
    private SpriteRenderer[] _sprites;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetWalking(bool walking)
    {
        if (_animator != null) _animator.SetBool(IsWalking, walking);
    }

    public void SetPunching(bool punching)
    {
        if (_animator != null) _animator.SetBool(IsPunch, punching);
    }
}