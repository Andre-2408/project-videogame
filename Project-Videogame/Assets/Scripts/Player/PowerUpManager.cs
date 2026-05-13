using UnityEngine;

public enum PowerUpType { None, SpreadShot, ZigzagShot, RapidFire }

/// <summary>
/// Gestiona el power-up activo del jugador.
/// Añade este componente al mismo prefab del Player.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    // ── Estado ──────────────────────────────────
    public PowerUpType ActiveType    { get; private set; } = PowerUpType.None;
    public float       RemainingTime { get; private set; } = 0f;
    public float       MaxDuration   { get; private set; } = 1f;
    public bool        HasPowerUp    => ActiveType != PowerUpType.None;

    // ── Eventos para el HUD ──────────────────────
    public event System.Action<PowerUpType, float> OnActivated;
    public event System.Action                     OnExpired;

    // ════════════════════════════════════════════
    void Update()
    {
        if (ActiveType == PowerUpType.None) return;

        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0f)
        {
            ActiveType    = PowerUpType.None;
            RemainingTime = 0f;
            OnExpired?.Invoke();
        }
    }

    // ── Activar power-up ─────────────────────────
    public void Activate(PowerUpType type, float duration)
    {
        ActiveType    = type;
        RemainingTime = duration;
        MaxDuration   = duration;
        OnActivated?.Invoke(type, duration);
    }

    // ── Fracción restante 0-1 (para barra de HUD) ─
    public float FillRatio => MaxDuration > 0 ? RemainingTime / MaxDuration : 0f;
}
