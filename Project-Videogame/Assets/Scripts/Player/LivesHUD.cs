using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra los iconos de vidas del jugador en el HUD.
///
/// SETUP en Unity Editor:
///   1. En la scene de juego, en el Canvas, crea un GameObject "LivesHUD"
///   2. Añade este script al GameObject
///   3. Abre el Life-sheet en el Sprite Editor y haz slice (Automatic)
///   4. Arrastra el sprite de vida (moneda del mono) al campo "lifeSprite"
///   5. Ajusta "maxLives" al mismo valor que PlayerHealth
/// </summary>
public class LivesHUD : MonoBehaviour
{
    [Header("Sprite del icono de vida (moneda del mono)")]
    [SerializeField] private Sprite lifeSprite;

    [Header("Configuración")]
    [SerializeField] private int   maxLives    = 3;
    [SerializeField] private float iconSize    = 32f;
    [SerializeField] private float iconSpacing = 8f;

    private Image[] _icons;

    // ════════════════════════════════════════════
    void Awake()
    {
        BuildIcons();
        PlayerHealth.OnLivesChanged += Refresh;
    }

    void Start()
    {
        Refresh(PlayerHealth.GetCurrentLives());
    }

    void OnDestroy()
    {
        PlayerHealth.OnLivesChanged -= Refresh;
    }

    // ════════════════════════════════════════════
    private void BuildIcons()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();

        // Ancla top-right, debajo de la barra de salud
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.offsetMin = new Vector2(10f, -96f);
        rt.offsetMax = new Vector2(200f, -60f);

        _icons = new Image[maxLives];
        for (int i = 0; i < maxLives; i++)
        {
            var go  = new GameObject("LifeIcon_" + i);
            go.transform.SetParent(transform, false);

            var iconRT = go.AddComponent<RectTransform>();
            iconRT.sizeDelta        = new Vector2(iconSize, iconSize);
            iconRT.anchorMin        = new Vector2(0f, 0.5f);
            iconRT.anchorMax        = new Vector2(0f, 0.5f);
            iconRT.pivot            = new Vector2(0f, 0.5f);
            iconRT.anchoredPosition = new Vector2(i * (iconSize + iconSpacing), 0f);

            var img = go.AddComponent<Image>();
            if (lifeSprite != null) img.sprite = lifeSprite;
            img.preserveAspect = true;

            _icons[i] = img;
        }
    }

    private void Refresh(int currentLives)
    {
        if (_icons == null) return;
        for (int i = 0; i < _icons.Length; i++)
            _icons[i].gameObject.SetActive(i < currentLives);
    }
}
