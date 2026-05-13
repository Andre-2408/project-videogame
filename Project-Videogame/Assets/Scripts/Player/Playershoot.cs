using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject bulletPrefab;
    public Transform  firePoint;

    private PlayerAnimationController _anim;
    private PowerUpManager            _powerUp;
    private bool _facingRight = true;

    // ════════════════════════════════════════════
    void Awake()
    {
        _anim    = GetComponent<PlayerAnimationController>();
        _powerUp = GetComponent<PowerUpManager>();
    }

    // ── Llamado desde PlayerMovement ─────────────
    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Solo dispara con pistola
        if (_anim != null && _anim.CurrentWeapon != PlayerAnimationController.Weapon.Gun)
            return;

        PowerUpType active = _powerUp != null ? _powerUp.ActiveType : PowerUpType.None;

        switch (active)
        {
            case PowerUpType.SpreadShot: ShootSpread(); break;
            case PowerUpType.ZigzagShot: ShootZigzag(); break;
            default:                     ShootNormal(); break;
        }
    }

    public void SetFacing(bool facingRight) => _facingRight = facingRight;

    // ════════════════════════════════════════════
    private void ShootNormal()
    {
        var b = SpawnBullet();
        if (b == null) return;
        b.GetComponent<Bullet>()?.SetDirection(FacingDir());
    }

    private void ShootSpread()
    {
        int[] angles = { -15, 0, 15 };
        float dir = FacingDir();
        foreach (int angleDeg in angles)
        {
            var b = SpawnBullet();
            if (b == null) continue;
            var bullet = b.GetComponent<Bullet>();
            if (bullet == null) continue;

            float rad = angleDeg * Mathf.Deg2Rad;
            var vel = new Vector2(
                Mathf.Cos(rad) * bullet.speed * dir,
                Mathf.Sin(rad) * bullet.speed
            );
            bullet.SetVelocity(vel);
        }
    }

    private void ShootZigzag()
    {
        var b = SpawnBullet();
        if (b == null) return;
        var bullet = b.GetComponent<Bullet>();
        if (bullet == null) return;
        bullet.mode = Bullet.BulletMode.Zigzag;
        bullet.SetDirection(FacingDir());
    }

    // ════════════════════════════════════════════
    private GameObject SpawnBullet()
    {
        return BulletPool.Instance != null
            ? BulletPool.Instance.GetPlayerBullet(firePoint.position, Quaternion.identity)
            : Object.Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    }

    private float FacingDir() => _facingRight ? 1f : -1f;
}
