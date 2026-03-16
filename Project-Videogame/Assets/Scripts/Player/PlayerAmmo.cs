using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerAmmo : MonoBehaviour
{
    [Header("Cargador")]
    [SerializeField] private int magazineSize  = 10;   // balas que caben en el cargador
    [SerializeField] private int startMagazine = 10;   // balas iniciales en el cargador

    [Header("Reserva (inventario)")]
    [SerializeField] private int maxReserve   = 90;    // tope maximo de reserva
    [SerializeField] private int startReserve = 20;    // balas iniciales en reserva

    [Header("Recarga")]
    [SerializeField] private float reloadTime = 1.5f;

    [Header("UI - Contador de balas (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI ammoText;  // ej: "8 | 22"

    private int  _magazine;       // balas actuales en el cargador
    private int  _reserve;        // balas en el inventario
    private bool _isReloading;

    public int  Magazine     => _magazine;
    public int  Reserve      => _reserve;
    public bool IsReloading  => _isReloading;
    public bool CanReload    => !_isReloading && _reserve > 0 && _magazine < magazineSize;

    void Awake()
    {
        _magazine = Mathf.Min(startMagazine, magazineSize);
        _reserve  = Mathf.Min(startReserve,  maxReserve);
        UpdateUI();
    }

    // Dispara una bala del cargador. Devuelve false si no hay balas o esta recargando.
    public bool TrySpend()
    {
        if (_isReloading || _magazine <= 0) return false;
        _magazine--;
        UpdateUI();
        return true;
    }

    // Agrega balas a la RESERVA (items del mapa).
    // Devuelve cuantas balas se aniadieron realmente.
    public int AddAmmo(int amount)
    {
        int before = _reserve;
        _reserve = Mathf.Min(_reserve + amount, maxReserve);
        UpdateUI();
        return _reserve - before;
    }

    // Intenta iniciar recarga. Devuelve false si no se puede.
    public bool TryReload(System.Action onComplete = null)
    {
        if (!CanReload) return false;
        StartCoroutine(ReloadCoroutine(onComplete));
        return true;
    }

    private IEnumerator ReloadCoroutine(System.Action onComplete)
    {
        _isReloading = true;

        if (ammoText != null)
            ammoText.text = "Recargando...";

        yield return new WaitForSeconds(reloadTime);

        // Cuantas balas necesita el cargador
        int needed = magazineSize - _magazine;
        int toLoad = Mathf.Min(needed, _reserve);

        _magazine    += toLoad;
        _reserve     -= toLoad;
        _isReloading  = false;

        UpdateUI();
        onComplete?.Invoke();
    }

    private void UpdateUI()
    {
        if (ammoText != null)
            ammoText.text = $"{_magazine} | {_reserve}";
    }
}
