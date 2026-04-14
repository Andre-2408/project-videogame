using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agrega este script al enemigo y al player.
/// Llama a TakeHit(direction) desde EnemyHealth o PlayerHealth al recibir daño.
/// direction: -1 = golpe vino de la izquierda (retrocede a la derecha), 1 = al revés.
/// </summary>
public class HitEffect : MonoBehaviour
{
    [Header("Flash rojo")]
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private Color hitColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Retroceso")]
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private float knockbackDuration = 0.12f;

    [Header("Sonido de daño")]
    [SerializeField] private AudioClip hitSound;

    private AudioSource _audio;
    private Rigidbody2D _rb;
    private List<SpriteRenderer> _sprites = new List<SpriteRenderer>();
    private Dictionary<SpriteRenderer, Color> _originalColors = new Dictionary<SpriteRenderer, Color>();
    private Coroutine _flashCoroutine;
    private Coroutine _knockbackCoroutine;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

        // Recoge todos los SpriteRenderers hijos
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            _sprites.Add(sr);
            _originalColors[sr] = sr.color;
        }
    }

    public void TakeHit(float hitDirectionX)
    {
        if (_audio != null && hitSound != null)
            _audio.PlayOneShot(hitSound);

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRed());

        if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
        _knockbackCoroutine = StartCoroutine(Knockback(hitDirectionX));
    }

    private IEnumerator FlashRed()
    {
        foreach (var sr in _sprites)
            if (sr != null) sr.color = hitColor;

        yield return new WaitForSeconds(flashDuration);

        foreach (var sr in _sprites)
            if (sr != null && _originalColors.ContainsKey(sr))
                sr.color = _originalColors[sr];
    }

    private IEnumerator Knockback(float hitDirectionX)
    {
        if (_rb == null) yield break;

        // Retrocede en dirección opuesta al golpe
        float knockDir = hitDirectionX >= 0 ? -1f : 1f;
        _rb.linearVelocity = new Vector2(knockDir * knockbackForce, _rb.linearVelocity.y);

        yield return new WaitForSeconds(knockbackDuration);

        // Frena el retroceso
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
    }
}