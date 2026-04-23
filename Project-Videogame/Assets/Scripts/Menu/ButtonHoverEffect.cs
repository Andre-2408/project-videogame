using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Agrega efecto de escala y sonido al hacer hover/click sobre cualquier botón.
/// Añade este componente al mismo GameObject que tenga un Button.
///
/// Úsalo tanto en el menú principal como en el menú de pausa.
/// Funciona correctamente aunque Time.timeScale = 0 (usa unscaledDeltaTime).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Escalas")]
    [SerializeField] private float hoverScale  = 1.07f;
    [SerializeField] private float pressScale  = 0.94f;
    [SerializeField] private float normalScale = 1.00f;
    [SerializeField] private float duration    = 0.10f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField][Range(0f, 1f)] private float volume = 0.7f;

    private RectTransform _rt;
    private Vector3       _baseScale;
    private Coroutine     _coroutine;
    private bool          _isHovering;
    private AudioSource   _audio;

    // ─────────────────────────────────────────
    void Awake()
    {
        _rt        = GetComponent<RectTransform>();
        _baseScale = _rt.localScale;

        if (hoverClip != null || clickClip != null)
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
        }
    }

    // ─────────────────────────────────────────
    public void OnPointerEnter(PointerEventData _)
    {
        _isHovering = true;
        ScaleTo(hoverScale);
        PlaySound(hoverClip);
    }

    public void OnPointerExit(PointerEventData _)
    {
        _isHovering = false;
        ScaleTo(normalScale);
    }

    public void OnPointerDown(PointerEventData _)
    {
        ScaleTo(pressScale);
    }

    public void OnPointerUp(PointerEventData _)
    {
        ScaleTo(_isHovering ? hoverScale : normalScale);
    }

    public void OnPointerClick(PointerEventData _)
    {
        PlaySound(clickClip);
    }

    // ─────────────────────────────────────────
    private void ScaleTo(float target)
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(ScaleCoroutine(target));
    }

    private IEnumerator ScaleCoroutine(float target)
    {
        Vector3 from = _rt.localScale;
        Vector3 to   = _baseScale * target;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _rt.localScale = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
        _rt.localScale = to;
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audio != null && clip != null)
            _audio.PlayOneShot(clip, volume);
    }
}
