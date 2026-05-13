using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Pantalla de victoria al completar el juego.
///
/// SETUP (una sola vez en Unity Editor):
///   1. Crea una nueva Scene llamada "WinnerScene" y añádela en Build Settings
///   2. En la Scene crea: Camera + Canvas (Screen Space - Overlay)
///   3. Añade este script al GameObject del Canvas
///   4. Importa el banner (mono+bananas) como Sprite y arrástralo a "winnerBanner"
///   5. Arrastra tu TMP_FontAsset a "gameFont" (opcional, usa default si no)
///   6. En el prefab del Boss Gorila (EnemyHealth): winnerScene = "WinnerScene"
/// </summary>
public class WinnerScreen : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Sprite winnerBanner;
    [SerializeField] private TMP_FontAsset gameFont;

    [Header("Escenas")]
    [SerializeField] private string firstLevel = "SampleScene";
    [SerializeField] private string menuScene  = "Menu";

    // ── colores ──────────────────────────────
    private static readonly Color Gold      = new Color(1.00f, 0.82f, 0.00f, 1f);
    private static readonly Color GoldHover = new Color(1.00f, 0.92f, 0.30f, 1f);
    private static readonly Color Red       = new Color(0.78f, 0.15f, 0.15f, 1f);
    private static readonly Color RedHover  = new Color(0.92f, 0.28f, 0.28f, 1f);
    private static readonly Color BgColor   = new Color(0.05f, 0.04f, 0.10f, 1f);

    private Image  _bannerImage;
    private bool   _transitioning;

    // ════════════════════════════════════════
    void Start()
    {
        BuildUI();
    }

    // ════════════════════════════════════════
    void BuildUI()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null) { Debug.LogError("WinnerScreen debe estar en el mismo GO que el Canvas."); return; }

        var canvasRT = GetComponent<RectTransform>();

        // Fondo oscuro
        var bg = MakeRect("Background", canvasRT);
        bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one;
        bg.offsetMin = bg.offsetMax = Vector2.zero;
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = BgColor;

        // Banner (imagen del mono)
        var bannerRT = MakeRect("WinnerBanner", canvasRT);
        bannerRT.anchorMin = new Vector2(0.5f, 0.55f);
        bannerRT.anchorMax = new Vector2(0.5f, 0.55f);
        bannerRT.sizeDelta = new Vector2(420, 220);
        bannerRT.anchoredPosition = Vector2.zero;
        bannerRT.localScale = Vector3.zero;
        _bannerImage = bannerRT.gameObject.AddComponent<Image>();
        if (winnerBanner != null) _bannerImage.sprite = winnerBanner;
        _bannerImage.preserveAspect = true;

        // Texto "YOU WIN!"
        var titleRT = MakeRect("TitleText", canvasRT);
        titleRT.anchorMin = new Vector2(0.5f, 0.87f);
        titleRT.anchorMax = new Vector2(0.5f, 0.87f);
        titleRT.sizeDelta = new Vector2(700, 90);
        titleRT.anchoredPosition = Vector2.zero;
        var titleTMP = titleRT.gameObject.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "YOU WIN!";
        titleTMP.fontSize = 72;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Gold;
        if (gameFont != null) titleTMP.font = gameFont;

        // Score
        int   savedScore = PlayerPrefs.GetInt(ScoreManager.PrefsScore, 0);
        float savedTime  = PlayerPrefs.GetFloat(ScoreManager.PrefsTime, 0f);

        var scoreRT = MakeRect("ScoreText", canvasRT);
        scoreRT.anchorMin = new Vector2(0.5f, 0.32f);
        scoreRT.anchorMax = new Vector2(0.5f, 0.32f);
        scoreRT.sizeDelta = new Vector2(500, 50);
        scoreRT.anchoredPosition = Vector2.zero;
        var scoreTMP = scoreRT.gameObject.AddComponent<TextMeshProUGUI>();
        scoreTMP.text = "PUNTUACIÓN: " + savedScore;
        scoreTMP.fontSize = 28;
        scoreTMP.fontStyle = FontStyles.Bold;
        scoreTMP.alignment = TextAlignmentOptions.Center;
        scoreTMP.color = Color.white;
        if (gameFont != null) scoreTMP.font = gameFont;

        // Tiempo
        var timeRT = MakeRect("TimeText", canvasRT);
        timeRT.anchorMin = new Vector2(0.5f, 0.25f);
        timeRT.anchorMax = new Vector2(0.5f, 0.25f);
        timeRT.sizeDelta = new Vector2(500, 44);
        timeRT.anchoredPosition = Vector2.zero;
        var timeTMP = timeRT.gameObject.AddComponent<TextMeshProUGUI>();
        timeTMP.text = "TIEMPO: " + ScoreManager.FormatTime(savedTime);
        timeTMP.fontSize = 26;
        timeTMP.alignment = TextAlignmentOptions.Center;
        timeTMP.color = new Color(0.8f, 0.8f, 0.8f);
        if (gameFont != null) timeTMP.font = gameFont;

        // Botones
        MakeButton("BtnPlayAgain", canvasRT, "JUGAR DE NUEVO",
                   new Vector2(0.35f, 0.10f), Gold, GoldHover, Color.black,
                   () => StartCoroutine(FadeAndLoad(firstLevel)));

        MakeButton("BtnMainMenu", canvasRT, "MENÚ PRINCIPAL",
                   new Vector2(0.65f, 0.10f), Red, RedHover, Color.white,
                   () => StartCoroutine(FadeAndLoad(menuScene)));

        // Animación de entrada del banner
        StartCoroutine(BannerEntrance(bannerRT));
    }

    // ════════════════════════════════════════
    IEnumerator BannerEntrance(RectTransform rt)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // ease-out cubic
            rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, ease);
            yield return null;
        }
        rt.localScale = Vector3.one;

        // Pequeño bounce al final
        yield return StartCoroutine(Bounce(rt));
    }

    IEnumerator Bounce(RectTransform rt)
    {
        float[] keys = { 1.08f, 0.96f, 1.03f, 1f };
        foreach (float s in keys)
        {
            float t = 0f;
            Vector3 start = rt.localScale;
            Vector3 end = Vector3.one * s;
            while (t < 1f)
            {
                t += Time.deltaTime * 10f;
                rt.localScale = Vector3.Lerp(start, end, t);
                yield return null;
            }
        }
    }

    IEnumerator FadeAndLoad(string scene)
    {
        if (_transitioning) yield break;
        _transitioning = true;

        var cg = gameObject.AddComponent<CanvasGroup>();
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        SceneManager.LoadScene(scene);
    }

    // ════════════════════════════════════════
    // ─── Helpers UI ─────────────────────────

    private RectTransform MakeRect(string name, RectTransform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    private void MakeButton(string name, RectTransform parent, string label,
                             Vector2 anchorCenter, Color bg, Color hover, Color txtColor,
                             System.Action onClick)
    {
        var rt = MakeRect(name, parent);
        rt.anchorMin = anchorCenter; rt.anchorMax = anchorCenter;
        rt.sizeDelta = new Vector2(240, 55);
        rt.anchoredPosition = Vector2.zero;

        var img = rt.gameObject.AddComponent<Image>();
        img.color = bg;

        var btn = rt.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = bg;
        colors.highlightedColor = hover;
        colors.pressedColor     = bg * 0.75f;
        colors.selectedColor    = bg;
        btn.colors = colors;
        btn.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));

        if (rt.gameObject.GetComponent<ButtonHoverEffect>() == null)
            rt.gameObject.AddComponent<ButtonHoverEffect>();

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(rt, false);
        var tRT = txtGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = txtColor;
        tmp.alignment = TextAlignmentOptions.Center;
        if (gameFont != null) tmp.font = gameFont;
    }
}
