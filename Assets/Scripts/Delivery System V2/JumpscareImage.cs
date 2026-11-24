using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Put on a UI Image (or a panel containing the image) under a Canvas.
/// Hook Show() into a JumpscareEvent's "On Triggered" list.
/// Uses a CanvasGroup (alpha) instead of SetActive so the coroutine keeps working.
[RequireComponent(typeof(CanvasGroup))]
public class JumpscareImage : MonoBehaviour
{
    [Tooltip("Optional. Only needed if you want to swap the sprite below.")]
    [SerializeField] private Image image;
    [Tooltip("Optional. One is picked at random each time. Leave empty to use the image as it is.")]
    [SerializeField] private Sprite[] sprites;

    [SerializeField] private float showSeconds = 0.6f;
    [SerializeField] private float fadeOutSeconds = 0.15f;

    [Header("Optional punch-in")]
    [SerializeField] private float startScale = 1.15f;
    [SerializeField] private float punchSeconds = 0.08f;

    private CanvasGroup group;
    private Coroutine routine;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    /// Call this from JumpscareEvent -> On Triggered.
    public void Show()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        if (image != null && sprites != null && sprites.Length > 0)
            image.sprite = sprites[Random.Range(0, sprites.Length)];

        group.alpha = 1f;
        transform.localScale = Vector3.one * startScale;

        // quick punch-in
        float t = 0f;
        while (t < punchSeconds)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, t / punchSeconds);
            yield return null;
        }
        transform.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(showSeconds);

        // fade out
        t = 0f;
        while (t < fadeOutSeconds)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = 1f - Mathf.Clamp01(t / fadeOutSeconds);
            yield return null;
        }
        group.alpha = 0f;
        routine = null;
    }
}