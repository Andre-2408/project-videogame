using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Mejora los botones existentes en la escena y añade el panel de Ajustes.
///
/// SETUP:
///   1. Arrastra tu botón JUGAR  → campo "Play Button"
///   2. Arrastra tu botón SALIR  → campo "Quit Button"
///   3. Asigna tu fuente         → campo "Game Font"
///   4. El botón AJUSTES y el panel de Ajustes se generan solos
/// </summary>
public class Menu : MonoBehaviour
{
    [Header("Botones existentes en la escena")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Primer nivel")]
    [SerializeField] private string firstLevel = "SampleScene";

    [Header("Fuente (la misma que usan ammoText / scoreText)")]
    [SerializeField] private TMP_FontAsset gameFont;

    [Header("Transición")]
    [SerializeField] private float transitionDuration = 0.22f;

    // ── colores ──────────────────────────────────
    private static readonly Color BtnGold      = new Color(1.00f, 0.82f, 0.00f, 1.00f);
    private static readonly Color BtnGoldHover = new Color(1.00f, 0.92f, 0.30f, 1.00f);
    private static readonly Color BtnRed       = new Color(0.78f, 0.15f, 0.15f, 1.00f);
    private static readonly Color BtnRedHover  = new Color(0.92f, 0.28f, 0.28f, 1.00f);

    // ── referencias generadas ──────────────────────────────
    private CanvasGroup   _settingsGroup;
    private RectTransform _settingsRect;
    private Slider        _musicSlider;
    private Slider        _sfxSlider;

    private bool      _transitioning;
    private const string MusicKey = "MusicVolume";
    private const string SfxKey   = "SfxVolume";

    // ════════════════════════════════════════════
    void Start()
    {
        // Mejorar botones existentes
        StyleButton(playButton, BtnGold, BtnGoldHover, Color.black);
        StyleButton(quitButton, BtnRed,  BtnRedHover,  Color.white);

        // Reemplazar eventos completos para limpiar listeners persistentes del Inspector
        if (playButton != null)  { playButton.onClick  = new Button.ButtonClickedEvent(); playButton.onClick.AddListener(PlayGame); }
        if (quitButton != null)  { quitButton.onClick  = new Button.ButtonClickedEvent(); quitButton.onClick.AddListener(QuitGame); }
        if (settingsButton != null)
        {
            // Reemplazar el evento completo elimina también los listeners persistentes del Inspector
            settingsButton.onClick = new Button.ButtonClickedEvent();
            settingsButton.onClick.AddListener(OpenSettings);
            StyleButton(settingsButton, BtnGold, BtnGoldHover, Color.black);
        }

        // Generar panel de ajustes (oculto por defecto)
        BuildSettingsPanel();

        // Cargar volúmenes guardados
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

    // ════════════════════════════════════════════
    // ─── Acciones ────────────────────────────────

    public void PlayGame()  => StartCoroutine(FadeAndLoad(firstLevel));
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenSettings()
    {
        if (_transitioning) return;
        StartCoroutine(ShowPanel(_settingsGroup, _settingsRect, true));
    }

    public void CloseSettings()
    {
        if (_transitioning) return;
        StartCoroutine(ShowPanel(_settingsGroup, _settingsRect, false));
    }

    // ════════════════════════════════════════════
    // ─── Mejora de botones existentes ────────────

    private void StyleButton(Button btn, Color bg, Color hover, Color txtColor)
    {
        if (btn == null) return;

        // Color de fondo
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = bg;

        // Color states
        var colors           = btn.colors;
        colors.normalColor      = bg;
        colors.highlightedColor = hover;
        colors.pressedColor     = bg * 0.75f;
        colors.selectedColor    = bg;
        btn.colors = colors;

        // Texto
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.color     = txtColor;
            tmp.fontStyle = FontStyles.Bold;
            if (gameFont != null) tmp.font = gameFont;
        }

        // Efecto hover si no lo tiene ya
        if (btn.GetComponent<ButtonHoverEffect>() == null)
            btn.gameObject.AddComponent<ButtonHoverEffect>();
    }

    // ════════════════════════════════════════════
    // ─── Panel de Ajustes (generado) ─────────────

    private void BuildSettingsPanel()
    {
        // Buscar el Canvas padre
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var canvasRT = canvas.GetComponent<RectTransform>();

        var go  = new GameObject("SettingsPanel");
        go.transform.SetParent(canvasRT, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.12f, 0.92f);

        _settingsRect = go.GetComponent<RectTransform>();
        _settingsRect.sizeDelta        = new Vector2(370, 330);
        _settingsRect.anchoredPosition = Vector2.zero;
        _settingsGroup = go.AddComponent<CanvasGroup>();

        // Layout
        var vl = go.AddComponent<VerticalLayoutGroup>();
        vl.padding              = new RectOffset(22, 22, 18, 18);
        vl.spacing              = 10;
        vl.childAlignment       = TextAnchor.UpperCenter;
        vl.childControlWidth    = true;
        vl.childControlHeight   = false;
        vl.childForceExpandWidth  = true;
        vl.childForceExpandHeight = false;

        MakeLabel(go.transform, "AJUSTES", 26, Color.white, 44);
        MakeSeparator(go.transform);
        MakeLabel(go.transform, "Música", 15, new Color(0.9f, 0.9f, 0.9f), 28);
        _musicSlider = MakeSlider(go.transform);
        MakeLabel(go.transform, "Efectos", 15, new Color(0.9f, 0.9f, 0.9f), 28);
        _sfxSlider = MakeSlider(go.transform);
        MakeSeparator(go.transform);
        MakePanelButton(go.transform, "VOLVER", BtnGold, BtnGoldHover, Color.black, CloseSettings);

        // Oculto por defecto
        _settingsGroup.alpha = 0f;
        _settingsGroup.interactable   = false;
        _settingsGroup.blocksRaycasts = false;
    }

    // ════════════════════════════════════════════
    // ─── Helpers de UI ───────────────────────────

    private void MakeLabel(Transform parent, string text, float size, Color color, float height)
    {
        var go  = new GameObject("Lbl");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0, height);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = FontStyles.Bold;
        tmp.color = color; tmp.alignment = TextAlignmentOptions.Center;
        if (gameFont != null) tmp.font = gameFont;
    }

    private void MakeSeparator(Transform parent)
    {
        var go  = new GameObject("Sep");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 2);
        go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
    }

    private void MakePanelButton(Transform parent, string label,
                                   Color bg, Color hover, Color txtColor, System.Action onClick)
    {
        var go  = new GameObject("Btn");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
        go.AddComponent<Image>().color = bg;

        var btn    = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = bg; colors.highlightedColor = hover;
        colors.pressedColor = bg * 0.75f; colors.selectedColor = bg;
        btn.colors = colors;
        btn.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tRT = txtGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 17; tmp.fontStyle = FontStyles.Bold;
        tmp.color = txtColor; tmp.alignment = TextAlignmentOptions.Center;
        if (gameFont != null) tmp.font = gameFont;

        go.AddComponent<ButtonHoverEffect>();
    }

    private Slider MakeSlider(Transform parent)
    {
        var go = new GameObject("Slider");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;

        var bgGO = new GameObject("BG"); bgGO.transform.SetParent(go.transform, false);
        var bgImg = bgGO.AddComponent<Image>(); bgImg.color = new Color(0.2f, 0.2f, 0.2f);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.25f); bgRT.anchorMax = new Vector2(1, 0.75f);
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

        var faGO = new GameObject("FillArea"); faGO.transform.SetParent(go.transform, false);
        var faRT = faGO.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0, 0.25f); faRT.anchorMax = new Vector2(1, 0.75f);
        faRT.offsetMin = new Vector2(5, 0); faRT.offsetMax = new Vector2(-15, 0);

        var fGO = new GameObject("Fill"); fGO.transform.SetParent(faGO.transform, false);
        var fImg = fGO.AddComponent<Image>(); fImg.color = BtnGold;
        var fRT = fGO.GetComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = new Vector2(1, 1);
        fRT.sizeDelta = new Vector2(10, 0); fRT.offsetMin = fRT.offsetMax = Vector2.zero;

        var haGO = new GameObject("HandleArea"); haGO.transform.SetParent(go.transform, false);
        var haRT = haGO.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(10, 0); haRT.offsetMax = new Vector2(-10, 0);

        var hGO = new GameObject("Handle"); hGO.transform.SetParent(haGO.transform, false);
        var hImg = hGO.AddComponent<Image>(); hImg.color = Color.white;
        var hRT = hGO.GetComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(20, 20); hRT.anchorMin = hRT.anchorMax = new Vector2(0.5f, 0.5f);

        slider.fillRect = fRT; slider.handleRect = hRT; slider.targetGraphic = hImg;
        return slider;
    }

    // ════════════════════════════════════════════
    // ─── Animaciones ─────────────────────────────

    private IEnumerator ShowPanel(CanvasGroup g, RectTransform rt, bool show)
    {
        _transitioning = true;
        float a0 = show ? 0f : 1f, a1 = show ? 1f : 0f;
        float s0 = show ? 0.88f : 1f, s1 = show ? 1f : 0.88f;
        float elapsed = 0f;

        g.alpha = a0; g.interactable = false; g.blocksRaycasts = false;
        rt.localScale = Vector3.one * s0;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);
            g.alpha       = Mathf.Lerp(a0, a1, t);
            rt.localScale = Vector3.one * Mathf.Lerp(s0, s1, t);
            yield return null;
        }

        g.alpha = a1; g.interactable = show; g.blocksRaycasts = show;
        rt.localScale = Vector3.one * s1;
        _transitioning = false;
    }

    private IEnumerator FadeAndLoad(string scene)
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / transitionDuration);
            yield return null;
        }
        SceneManager.LoadScene(scene);
    }
}
