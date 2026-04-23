using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class HealthBarUI : MonoBehaviour
{
    [Header("Fuente (opcional)")]
    [SerializeField] private TMP_FontAsset gameFont;

    [Header("Animación")]
    [SerializeField] private float fillSpeed = 1.5f;

    // ── colores ─────────────────────────────────
    private static readonly Color ColorHigh  = new Color(0.18f, 0.88f, 0.20f);
    private static readonly Color ColorMid   = new Color(1.00f, 0.75f, 0.05f);
    private static readonly Color ColorLow   = new Color(0.92f, 0.15f, 0.15f);
    private static readonly Color PanelColor = new Color(0.04f, 0.04f, 0.08f, 0.88f);
    private static readonly Color BarBgColor = new Color(0.12f, 0.12f, 0.12f, 1.00f);

    // ── referencias ─────────────────────────────
    private RectTransform _fillRect;   // se escala horizontalmente
    private Image         _fillImage;
    private PlayerHealth  _playerHealth;
    private float         _targetFill  = 1f;
    private float         _currentFill = 1f;

    // ════════════════════════════════════════════
    void Awake() => BuildUI();

    void Start()
    {
        _playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged += OnHealthChanged;
            OnHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            _currentFill = _targetFill;
            ApplyFill(_currentFill);
        }
    }

    void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    void Update()
    {
        if (_fillRect == null) return;
        if (Mathf.Abs(_currentFill - _targetFill) > 0.001f)
        {
            _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.deltaTime * fillSpeed);
            ApplyFill(_currentFill);
        }
    }

    // Mueve el borde derecho del fill — así se achica físicamente
    private void ApplyFill(float t)
    {
        // anchorMax.x controla hasta dónde llega la barra (0=nada, 1=todo)
        _fillRect.anchorMax = new Vector2(t, 1f);
        if (_fillImage != null)
            _fillImage.color = GetBarColor(t);
    }

    // ════════════════════════════════════════════
    private void OnHealthChanged(int current, int max)
    {
        _targetFill = max > 0 ? (float)current / max : 0f;
    }

    private Color GetBarColor(float fill)
    {
        if (fill > 0.55f) return Color.Lerp(ColorMid, ColorHigh, (fill - 0.55f) / 0.45f);
        if (fill > 0.25f) return Color.Lerp(ColorLow, ColorMid,  (fill - 0.25f) / 0.30f);
        return ColorLow;
    }

    // ════════════════════════════════════════════
    private void BuildUI()
    {
        // Panel principal — ancla top-left, 36% del ancho
        var rt       = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0.36f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.offsetMin = new Vector2(10f, -54f);
        rt.offsetMax = new Vector2(-10f, -10f);
        gameObject.AddComponent<Image>().color = PanelColor;

        // Corazón ♥
        var hGO  = new GameObject("Heart");
        hGO.transform.SetParent(transform, false);
        var hRT  = hGO.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0f, 0f);
        hRT.anchorMax = new Vector2(0f, 1f);
        hRT.offsetMin = new Vector2(6f, 2f);
        hRT.offsetMax = new Vector2(42f, -2f);
        var hTxt = hGO.AddComponent<TextMeshProUGUI>();
        hTxt.text = "♥"; hTxt.fontSize = 22f;
        hTxt.color = ColorHigh;
        hTxt.alignment = TextAlignmentOptions.Center;
        if (gameFont != null) hTxt.font = gameFont;

        // Fondo oscuro de la barra
        var bgGO = new GameObject("BarBg");
        bgGO.transform.SetParent(transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0f);
        bgRT.anchorMax = new Vector2(1f, 1f);
        bgRT.offsetMin = new Vector2(48f, 8f);
        bgRT.offsetMax = new Vector2(-8f, -8f);
        bgGO.AddComponent<Image>().color = BarBgColor;

        // Fill — usa anchorMax.x para achicarse físicamente
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform, false);
        _fillRect = fillGO.AddComponent<RectTransform>();
        _fillRect.anchorMin = new Vector2(0f, 0f);
        _fillRect.anchorMax = new Vector2(1f, 1f);  // empieza completo
        _fillRect.offsetMin = _fillRect.offsetMax = Vector2.zero;
        _fillImage = fillGO.AddComponent<Image>();
        _fillImage.color = ColorHigh;
    }
}
