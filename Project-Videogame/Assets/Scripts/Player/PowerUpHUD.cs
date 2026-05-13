using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra el power-up activo y su tiempo restante en el HUD.
///
/// SETUP en Unity Editor:
///   1. En el Canvas de juego, crea un GameObject "PowerUpHUD"
///   2. Añade este script
///   3. (Opcional) arrastra sprites de power-up a "powerUpSprites"
///      índices: 0=SpreadShot, 1=ZigzagShot, 2=RapidFire
/// </summary>
public class PowerUpHUD : MonoBehaviour
{
    [Header("Sprites por tipo (0=Spread, 1=Zigzag, 2=Rapid)")]
    [SerializeField] private Sprite[] powerUpSprites;

    [Header("Fuente (opcional)")]
    [SerializeField] private TMP_FontAsset gameFont;

    private PowerUpManager _powerUp;
    private Image          _iconImage;
    private Image          _barFill;
    private TextMeshProUGUI _label;
    private CanvasGroup    _group;

    private static readonly Color ColorSpread = new Color(0.20f, 0.80f, 1.00f);
    private static readonly Color ColorZigzag = new Color(1.00f, 0.82f, 0.00f);
    private static readonly Color ColorRapid  = new Color(1.00f, 0.35f, 0.10f);

    // ════════════════════════════════════════════
    void Awake()
    {
        BuildUI();
    }

    void Start()
    {
        _powerUp = FindFirstObjectByType<PowerUpManager>();
        if (_powerUp != null)
        {
            _powerUp.OnActivated += Show;
            _powerUp.OnExpired   += Hide;
        }
        if (_group != null) _group.alpha = 0f;
    }

    void OnDestroy()
    {
        if (_powerUp != null)
        {
            _powerUp.OnActivated -= Show;
            _powerUp.OnExpired   -= Hide;
        }
    }

    void Update()
    {
        if (_powerUp == null || !_powerUp.HasPowerUp) return;
        if (_barFill != null)
            _barFill.fillAmount = _powerUp.FillRatio;
    }

    // ════════════════════════════════════════════
    private void Show(PowerUpType type, float duration)
    {
        if (_group != null) _group.alpha = 1f;

        // Etiqueta
        string[] names = { "", "SPREAD", "ZIGZAG", "RAPID" };
        if (_label != null) _label.text = names[(int)type];

        // Color de la barra
        Color barColor = type switch
        {
            PowerUpType.SpreadShot => ColorSpread,
            PowerUpType.ZigzagShot => ColorZigzag,
            PowerUpType.RapidFire  => ColorRapid,
            _                      => Color.white
        };
        if (_barFill != null) _barFill.color = barColor;

        // Icono
        if (_iconImage != null && powerUpSprites != null)
        {
            int idx = (int)type - 1;
            _iconImage.sprite = (idx >= 0 && idx < powerUpSprites.Length) ? powerUpSprites[idx] : null;
            _iconImage.enabled = _iconImage.sprite != null;
        }

        if (_barFill != null) _barFill.fillAmount = 1f;
    }

    private void Hide()
    {
        if (_group != null) _group.alpha = 0f;
    }

    // ════════════════════════════════════════════
    private void BuildUI()
    {
        _group = gameObject.AddComponent<CanvasGroup>();

        var rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();

        // Posición: top-right
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(-180f, -56f);
        rt.offsetMax = new Vector2(-10f,  -10f);

        var bg = MakeRect("BG", rt);
        bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one;
        bg.offsetMin = bg.offsetMax = Vector2.zero;
        bg.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.10f, 0.88f);

        // Icono (izquierda)
        var iconRT = MakeRect("Icon", rt);
        iconRT.anchorMin = new Vector2(0f, 0.1f);
        iconRT.anchorMax = new Vector2(0f, 0.9f);
        iconRT.offsetMin = new Vector2(6f, 0f);
        iconRT.offsetMax = new Vector2(42f, 0f);
        _iconImage = iconRT.gameObject.AddComponent<Image>();
        _iconImage.preserveAspect = true;

        // Nombre del power-up
        var lblRT = MakeRect("Label", rt);
        lblRT.anchorMin = new Vector2(0f, 0.55f);
        lblRT.anchorMax = new Vector2(1f, 1f);
        lblRT.offsetMin = new Vector2(48f, 2f);
        lblRT.offsetMax = new Vector2(-6f, -2f);
        _label = lblRT.gameObject.AddComponent<TextMeshProUGUI>();
        _label.fontSize = 13; _label.fontStyle = FontStyles.Bold;
        _label.color = Color.white; _label.alignment = TextAlignmentOptions.Left;
        if (gameFont != null) _label.font = gameFont;

        // Fondo barra
        var barBgRT = MakeRect("BarBG", rt);
        barBgRT.anchorMin = new Vector2(0f, 0.05f);
        barBgRT.anchorMax = new Vector2(1f, 0.5f);
        barBgRT.offsetMin = new Vector2(48f, 4f);
        barBgRT.offsetMax = new Vector2(-6f, -2f);
        barBgRT.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

        // Fill de la barra
        var fillRT = MakeRect("Fill", barBgRT);
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        _barFill = fillRT.gameObject.AddComponent<Image>();
        _barFill.type = Image.Type.Filled;
        _barFill.fillMethod = Image.FillMethod.Horizontal;
        _barFill.fillAmount = 1f;
        _barFill.color = ColorSpread;
    }

    private RectTransform MakeRect(string name, RectTransform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }
}
