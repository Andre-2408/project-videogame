using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Pantalla de Game Over.
///
/// SETUP (una sola vez en Unity Editor):
///   1. File → New Scene → "GameOverScene" → añadir en Build Settings
///   2. En la Scene: Camera + Canvas (Screen Space - Overlay)
///   3. Añade este script al mismo GameObject del Canvas
///   4. (Opcional) arrastra tu TMP_FontAsset a "gameFont"
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("Fuente (opcional)")]
    [SerializeField] private TMP_FontAsset gameFont;

    [Header("Escenas")]
    [SerializeField] private string menuScene = "Menu";

    private bool _transitioning;

    private static readonly Color Red       = new Color(0.80f, 0.10f, 0.10f, 1f);
    private static readonly Color RedHover  = new Color(0.92f, 0.28f, 0.28f, 1f);
    private static readonly Color Gold      = new Color(1.00f, 0.82f, 0.00f, 1f);
    private static readonly Color GoldHover = new Color(1.00f, 0.92f, 0.30f, 1f);
    private static readonly Color BgColor   = new Color(0.04f, 0.02f, 0.02f, 1f);

    // ════════════════════════════════════════════
    void Start() => BuildUI();

    void BuildUI()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null) { Debug.LogError("GameOverScreen debe estar en el mismo GO que el Canvas."); return; }
        var canvasRT = GetComponent<RectTransform>();

        // Fondo
        var bg = Make("Background", canvasRT);
        bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one;
        bg.offsetMin = bg.offsetMax = Vector2.zero;
        bg.gameObject.AddComponent<Image>().color = BgColor;

        // Título "GAME OVER" (parpadea)
        var titleRT = Make("Title", canvasRT);
        titleRT.anchorMin = new Vector2(0.5f, 0.62f);
        titleRT.anchorMax = new Vector2(0.5f, 0.62f);
        titleRT.sizeDelta = new Vector2(700, 110);
        titleRT.anchoredPosition = Vector2.zero;
        var titleTMP = titleRT.gameObject.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "GAME OVER";
        titleTMP.fontSize = 82;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Red;
        if (gameFont != null) titleTMP.font = gameFont;
        StartCoroutine(Blink(titleTMP));

        // Puntuación guardada
        int savedScore = PlayerPrefs.GetInt(ScoreManager.PrefsScore, 0);
        AddLabel(canvasRT, "Puntuación: " + savedScore, new Vector2(0.5f, 0.47f), 28);

        // Botón CONTINUAR (recarga el último nivel)
        string lastScene = PlayerPrefs.GetString(PlayerHealth.LastSceneKey, "SampleScene");
        MakeButton("BtnContinue", canvasRT, "CONTINUAR",
            new Vector2(0.35f, 0.28f), Gold, GoldHover, Color.black,
            () => StartCoroutine(FadeLoad(lastScene)));

        // Botón MENÚ
        MakeButton("BtnMenu", canvasRT, "MENÚ PRINCIPAL",
            new Vector2(0.65f, 0.28f), Red, RedHover, Color.white,
            () => StartCoroutine(FadeLoad(menuScene)));
    }

    // ════════════════════════════════════════════
    IEnumerator Blink(TextMeshProUGUI tmp)
    {
        while (true)
        {
            yield return Lerp01(tmp, 1f, 0.3f, 1.8f);
            yield return Lerp01(tmp, 0.3f, 1f, 1.8f);
        }
    }

    IEnumerator Lerp01(TextMeshProUGUI tmp, float from, float to, float speed)
    {
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime * speed; tmp.alpha = Mathf.Lerp(from, to, t); yield return null; }
    }

    IEnumerator FadeLoad(string scene)
    {
        if (_transitioning) yield break;
        _transitioning = true;
        var cg = gameObject.AddComponent<CanvasGroup>();
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime * 4f; cg.alpha = Mathf.Lerp(1f, 0f, t); yield return null; }
        PlayerHealth.InitNewGame();     // resetea vidas al continuar
        SceneManager.LoadScene(scene);
    }

    // ════════════════════════════════════════════
    private RectTransform Make(string name, RectTransform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    private void AddLabel(RectTransform parent, string text, Vector2 anchor, float size)
    {
        var rt = Make("Lbl", parent);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(500, 44);
        rt.anchoredPosition = Vector2.zero;
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.85f, 0.85f, 0.85f);
        if (gameFont != null) tmp.font = gameFont;
    }

    private void MakeButton(string name, RectTransform parent, string label,
        Vector2 anchor, Color bg, Color hover, Color txtColor, System.Action onClick)
    {
        var rt = Make(name, parent);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(240, 55);
        rt.anchoredPosition = Vector2.zero;
        rt.gameObject.AddComponent<Image>().color = bg;

        var btn = rt.gameObject.AddComponent<Button>();
        var c = btn.colors;
        c.normalColor = bg; c.highlightedColor = hover;
        c.pressedColor = bg * 0.75f; c.selectedColor = bg;
        btn.colors = c;
        btn.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));
        if (rt.gameObject.GetComponent<ButtonHoverEffect>() == null)
            rt.gameObject.AddComponent<ButtonHoverEffect>();

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(rt, false);
        var tRT = txtGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 18; tmp.fontStyle = FontStyles.Bold;
        tmp.color = txtColor; tmp.alignment = TextAlignmentOptions.Center;
        if (gameFont != null) tmp.font = gameFont;
    }
}
