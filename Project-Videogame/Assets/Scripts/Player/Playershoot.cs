using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint; // punto de salida de la bala

    private PlayerAnimationController _anim;
    private bool _facingRight = true;

    void Awake()
    {
        _anim = GetComponent<PlayerAnimationController>();
    }

    // Llamado desde PlayerMovement cuando se hace click izquierdo
    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Solo dispara si tiene pistola equipada
        if (_anim != null && _anim.CurrentWeapon != PlayerAnimationController.Weapon.Gun)
            return;

        float direction = _anim != null
            ? (_anim.CurrentWeapon == PlayerAnimationController.Weapon.Gun ? GetFacingDirection() : 0f)
            : GetFacingDirection();

        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = b.GetComponent<Bullet>();
        if (bullet != null)
            bullet.SetDirection(direction);
    }

    public void SetFacing(bool facingRight)
    {
        _facingRight = facingRight;
    }

    private float GetFacingDirection()
    {
        return _facingRight ? 1f : -1f;
    }
}