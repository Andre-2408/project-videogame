using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

/// <summary>
/// Menú de pausa con UI construida completamente por código.
///
/// SETUP (muy simple):
///   1. Borra todo el contenido actual de PauseMenuRoot (Overlay, Panel, ConfirmPanel)
///   2. Deja PauseMenuRoot vacío con solo este script
///   3. Asigna tu fuente en el campo "Game Font" (la misma de scoreText / ammoText)
///   4. Dale Play y presiona ESC
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PauseMenu : MonoBehaviour
{
    [Header("Fuente (arrastra la misma que usan ammoText / scoreText)")]
    [SerializeField] private TMP_FontAsset gameFont;

    [Header("Animación")]
    [SerializeField] private float animDuration   = 0.20f;
    [SerializeField] private float panelScaleFrom = 0.85f;

    // ── estado ──────────────────────────────────
    public static bool IsPaused { get; private set; }
    private static PauseMenu _instance;
    private bool      _confirmVisible;
    private bool      _settingsVisible;
    private Coroutine _anim;

    // ── referencias generadas ────────────────────
    private CanvasGroup    _rootGroup;
    private CanvasGroup    _mainGroup;
    private RectTransform  _mainRect;
    private GameObject     _confirmGO;
    private CanvasGroup    _confirmGroup;
    private RectTransform  _confirmRect;
    private GameObject     _settingsGO;
    private CanvasGroup    _settingsGroup;
    private RectTransform  _settingsRect;
    private Slider         _musicSlider;
    private Slider         _sfxSlider;

    private const string MusicKey = "MusicVolume";
    private const string SfxKey   = "SfxVolume";

    // ── colores ──────────────────────────────────
    private static readonly Color PanelBg      = new Color(0.07f, 0.07f, 0.15f, 0.97f);
    private static readonly Color BtnGold      = new Color(1.00f, 0.82f, 0.00f, 1.00f);
    private static readonly Color BtnGoldHover = new Color(1.00f, 0.92f, 0.30f, 1.00f);
    private static readonly Color BtnRed       = new Color(0.78f, 0.15f, 0.15f, 1.00f);
    private static readonly Color BtnRedHover  = new Color(0.92f, 0.28f, 0.28f, 1.00f);
    private static readonly Color BtnGreen     = new Color(0.18f, 0.58f, 0.18f, 1.00f);
    private static readonly Color BtnGreenHover= new Color(0.28f, 0.72f, 0.28f, 1.00f);

    // ════════════════════════════════════════════
    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        BuildUI();
    }

    void Start()
    {
        SetRootVisible(false);
        _confirmGO.SetActive(false);
        _settingsGO.SetActive(false);

        if (_musicSlider != null)
        {
            _musicSlider.value = PlayerPrefs.GetFloat(MusicKey, 1f);
            _musicSlider.onValueChanged.AddListener(v => { PlayerPrefs.SetFloat(MusicKey, v); AudioListener.volume = v; });
        }
        if (_sfxSlider != null)
        {
            _sfxSlider.value = PlayerPrefs.GetFloat(SfxKey, 1f);
            _sfxSlider.onValueChanged.AddListener(v => PlayerPrefs.SetFloat(SfxKey, v));
        }
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
        if (IsPaused) { Time.timeScale = 1f; IsPaused = false; }
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_confirmVisible) CancelQuit();
            else if (_settingsVisible) ClosePauseSettings();
            else                 TogglePause();
        }
    }

    // ════════════════════════════════════════════
    // ─── API pública ─────────────────────────────

    public void TogglePause() { if (IsPaused) Resume(); else Pause(); }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        SetPlayerMovement(false);
        SetRootVisible(true);
        Animate(_mainGroup, _mainRect, true);
    }

    public void Resume()
    {
        Animate(_mainGroup, _mainRect, false, () =>
        {
            IsPaused = false;
            Time.timeScale = 1f;
            SetPlayerMovement(true);
            SetRootVisible(false);
        });
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; IsPaused = false;
        SceneManager.LoadScene("Menu");
    }

    public void AskQuit()
    {
        SetInteractable(_mainGroup, false);
        StartCoroutine(FadeAlpha(_mainGroup, 1f, 0.20f));
        _confirmGO.SetActive(true);
        SetGroupInstant(_confirmGroup, false);
        Animate(_confirmGroup, _confirmRect, true);
        _confirmVisible = true;
    }

    public void ConfirmQuit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void CancelQuit()
    {
        Animate(_confirmGroup, _confirmRect, false, () =>
        {
            _confirmGO.SetActive(false);
            _confirmVisible = false;
            SetInteractable(_mainGroup, true);
            StartCoroutine(FadeAlpha(_mainGroup, _mainGroup.alpha, 1f));
        });
    }

    public void OpenPauseSettings()
    {
        SetInteractable(_mainGroup, false);
        StartCoroutine(FadeAlpha(_mainGroup, 1f, 0.20f));
        _settingsGO.SetActive(true);
        SetGroupInstant(_settingsGroup, false);
        Animate(_settingsGroup, _settingsRect, true);
        _settingsVisible = true;
    }

    public void ClosePauseSettings()
    {
        Animate(_settingsGroup, _settingsRect, false, () =>
        {
            _settingsGO.SetActive(false);
            _settingsVisible = false;
            SetInteractable(_mainGroup, true);
            StartCoroutine(FadeAlpha(_mainGroup, _mainGroup.alpha, 1f));
        });
    }

    // ════════════════════════════════════════════
    // ─── Construcción de la UI ───────────────────

    private void BuildUI()
    {
        _rootGroup = GetComponent<CanvasGroup>();

        // ── Overlay (fondo oscuro pantalla completa) ──────────
        MakeOverlay();

        // ── Panel principal ───────────────────────────────────
        var mainGO = MakePanel(new Vector2(390, 400), out _mainGroup, out _mainRect);

        AddLayout(mainGO, vertical: true, padH: 24, padV: 24, spacing: 10);
        MakeTitle(mainGO.transform, "PAUSA");
        MakeButton(mainGO.transform, "CONTINUAR",      BtnGold,  BtnGoldHover, Color.black, Resume);
        MakeButton(mainGO.transform, "REINICIAR",      BtnGold,  BtnGoldHover, Color.black, RestartLevel);
        MakeButton(mainGO.transform, "MENÚ PRINCIPAL", BtnGold,  BtnGoldHover, Color.black, GoToMainMenu);
        MakeButton(mainGO.transform, "AJUSTES",        BtnGold,  BtnGoldHover, Color.black, OpenPauseSettings);
        MakeButton(mainGO.transform, "SALIR",          BtnRed,   BtnRedHover,  Color.white, AskQuit);

        // ── Panel de ajustes ──────────────────────────────────
        _settingsGO = MakePanel(new Vector2(390, 360), out _settingsGroup, out _settingsRect);
        AddLayout(_settingsGO, vertical: true, padH: 24, padV: 22, spacing: 10);
        MakeTitle(_settingsGO.transform, "AJUSTES");
        MakeText(_settingsGO.transform, "Música", 15, new Color(0.9f,0.9f,0.9f), 28);
        _musicSlider = MakeSlider(_settingsGO.transform);
        MakeText(_settingsGO.transform, "Efectos de sonido", 15, new Color(0.9f,0.9f,0.9f), 28);
        _sfxSlider = MakeSlider(_settingsGO.transform);
        MakeButton(_settingsGO.transform, "VOLVER", BtnGold, BtnGoldHover, Color.black, ClosePauseSettings);

        // ── Panel de confirmación ─────────────────────────────
        _confirmGO = MakePanel(new Vector2(390, 190), out _confirmGroup, out _confirmRect);

        AddLayout(_confirmGO, vertical: true, padH: 22, padV: 20, spacing: 12);
        MakeText(_confirmGO.transform, "¿Seguro que quieres salir?", 17, Color.white, 54);

        var rowGO = MakeRow(_confirmGO.transform);
        MakeButtonFixed(rowGO.transform, "SÍ", 130, BtnRed,   BtnRedHover,   Color.white, ConfirmQuit);
        MakeButtonFixed(rowGO.transform, "NO", 130, BtnGreen, BtnGreenHover, Color.white, CancelQuit);
    }

    // ─── helpers de construcción ─────────────────

    private void MakeOverlay()
    {
        var go  = new GameObject("Overlay");
        go.transform.SetParent(transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.62f);
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private GameObject MakePanel(Vector2 size, out CanvasGroup cg, out RectTransform rt)
    {
        var go  = new GameObject("Panel");
        go.transform.SetParent(transform, false);
        var img = go.AddComponent<Image>();
        img.color = PanelBg;
        rt = go.GetComponent<RectTransform>();
        rt.sizeDelta       = size;
        rt.anchoredPosition = Vector2.zero;
        cg = go.AddComponent<CanvasGroup>();
        return go;
    }

    private void AddLayout(GameObject go, bool vertical, int padH, int padV, int spacing)
    {
        if (vertical)
        {
            var vl = go.AddComponent<VerticalLayoutGroup>();
            vl.padding              = new RectOffset(padH, padH, padV, padV);
            vl.spacing              = spacing;
            vl.childAlignment       = TextAnchor.UpperCenter;
            vl.childControlWidth    = true;
            vl.childControlHeight   = false;
            vl.childForceExpandWidth  = true;
            vl.childForceExpandHeight = false;
        }
    }

    private GameObject MakeRow(Transform parent)
    {
        var go  = new GameObject("BtnRow");
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 52);
        var hl  = go.AddComponent<HorizontalLayoutGroup>();
        hl.spacing              = 14;
        hl.childAlignment       = TextAnchor.MiddleCenter;
        hl.childControlWidth    = true;
        hl.childControlHeight   = false;
        hl.childForceExpandWidth  = true;
        hl.childForceExpandHeight = false;
        return go;
    }

    private Slider MakeSlider(Transform parent)
    {
        var go = new GameObject("Slider");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;

        var bgGO = new GameObject("BG"); bgGO.transform.SetParent(go.transform, false);
        bgGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0,.25f); bgRT.anchorMax = new Vector2(1,.75f);
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

        var faGO = new GameObject("FillArea"); faGO.transform.SetParent(go.transform, false);
        var faRT = faGO.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0,.25f); faRT.anchorMax = new Vector2(1,.75f);
        faRT.offsetMin = new Vector2(5,0); faRT.offsetMax = new Vector2(-15,0);

        var fGO = new GameObject("Fill"); fGO.transform.SetParent(faGO.transform, false);
        var fImg = fGO.AddComponent<Image>(); fImg.color = BtnGold;
        var fRT = fGO.GetComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = new Vector2(1,1);
        fRT.sizeDelta = new Vector2(10,0); fRT.offsetMin = fRT.offsetMax = Vector2.zero;

        var haGO = new GameObject("HandleArea"); haGO.transform.SetParent(go.transform, false);
        var haRT = haGO.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(10,0); haRT.offsetMax = new Vector2(-10,0);

        var hGO = new GameObject("Handle"); hGO.transform.SetParent(haGO.transform, false);
        var hImg = hGO.AddComponent<Image>(); hImg.color = Color.white;
        var hRT = hGO.GetComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(20,20); hRT.anchorMin = hRT.anchorMax = new Vector2(.5f,.5f);

        slider.fillRect = fRT; slider.handleRect = hRT; slider.targetGraphic = hImg;
        return slider;
    }

    private void MakeTitle(Transform parent, string text)
    {
        var go  = new GameObject("Title");
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 58);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 40;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (gameFont != null) tmp.font = gameFont;
    }

    private void MakeText(Transform parent, string text, float size, Color color, float height)
    {
        var go  = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, height);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text              = text;
        tmp.fontSize          = size;
        tmp.color             = color;
        tmp.alignment         = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        if (gameFont != null) tmp.font = gameFont;
    }

    private void MakeButton(Transform parent, string label, Color bg, Color hover, Color txtColor,
                             System.Action onClick)
    {
        var go  = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 50);

        var img = go.AddComponent<Image>();
        img.color = bg;

        var btn    = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = bg;
        colors.highlightedColor = hover;
        colors.pressedColor     = bg * 0.75f;
        colors.selectedColor    = bg;
        btn.colors = colors;
        btn.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));

        // texto
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tRT   = txtGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tmp   = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 17;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = txtColor;
        tmp.alignment = TextAlignmentOptions.Center;
        if (gameFont != null) tmp.font = gameFont;

        // efecto hover
        go.AddComponent<ButtonHoverEffect>();
    }

    // Botón con ancho fijo (para la fila SÍ/NO)
    private void MakeButtonFixed(Transform parent, string label, float width,
                                  Color bg, Color hover, Color txtColor, System.Action onClick)
    {
        var go  = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, 50);

        // LayoutElement con ancho preferido fijo
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth  = width;
        le.preferredHeight = 50;
        le.flexibleWidth   = 0;

        var img = go.AddComponent<Image>();
        img.color = bg;

        var btn    = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = bg;
        colors.highlightedColor = hover;
        colors.pressedColor     = bg * 0.75f;
        colors.selectedColor    = bg;
        btn.colors = colors;
        btn.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tRT   = txtGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tmp   = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 17;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = txtColor;
        tmp.alignment = TextAlignmentOptions.Center;
        if (gameFont != null) tmp.font = gameFont;

        go.AddComponent<ButtonHoverEffect>();
    }

    // ════════════════════════════════════════════
    // ─── Helpers de estado ───────────────────────

    private void SetRootVisible(bool show)
    {
        _rootGroup.alpha          = show ? 1f : 0f;
        _rootGroup.interactable   = show;
        _rootGroup.blocksRaycasts = show;
    }

    private void SetGroupInstant(CanvasGroup g, bool show)
    {
        g.alpha = show ? 1f : 0f; g.interactable = show; g.blocksRaycasts = show;
    }

    private void SetInteractable(CanvasGroup g, bool v)
    {
        g.interactable = v; g.blocksRaycasts = v;
    }

    private void SetPlayerMovement(bool enabled)
    {
        var m = FindFirstObjectByType<PlayerMovement>();
        if (m != null) m.enabled = enabled;
    }

    // ════════════════════════════════════════════
    // ─── Animaciones ─────────────────────────────

    private void Animate(CanvasGroup g, RectTransform rt, bool show, System.Action onDone = null)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimCo(g, rt, show, onDone));
    }

    private IEnumerator AnimCo(CanvasGroup g, RectTransform rt, bool show, System.Action onDone)
    {
        float a0 = show ? 0f : 1f,           a1 = show ? 1f : 0f;
        float s0 = show ? panelScaleFrom : 1f, s1 = show ? 1f : panelScaleFrom;
        float elapsed = 0f;

        g.alpha = a0; g.interactable = false; g.blocksRaycasts = false;
        rt.localScale = Vector3.one * s0;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            g.alpha       = Mathf.Lerp(a0, a1, t);
            rt.localScale = Vector3.one * Mathf.Lerp(s0, s1, t);
            yield return null;
        }

        g.alpha = a1; g.interactable = show; g.blocksRaycasts = show;
        rt.localScale = Vector3.one * s1;
        onDone?.Invoke();
    }

    private IEnumerator FadeAlpha(CanvasGroup g, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / animDuration));
            yield return null;
        }
        g.alpha = to;
    }
}
