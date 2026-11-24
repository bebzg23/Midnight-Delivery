using System.Collections;
using UnityEngine;

/// Plays a looping ambient sound while a night is active.
/// Starts when the night starts, fades out when it is completed or failed.
[RequireComponent(typeof(AudioSource))]
public class NightAmbience : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    [SerializeField] private float fadeInSeconds = 2f;
    [SerializeField] private float fadeOutSeconds = 1.5f;

    private AudioSource source;
    private Coroutine fade;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
    }

    private void Start()
    {
        var dm = DeliveryManager.Instance;
        dm.onNightStarted.AddListener(_ => StartAmbience());
        dm.onNightCompleted.AddListener(StopAmbience);
        dm.onNightFailed.AddListener(StopAmbience);

        // DeliveryManager may have started the night before this Start ran.
        if (dm.NightActive) StartAmbience();
    }

    private void StartAmbience()
    {
        if (!source.isPlaying) source.Play();
        StartFade(volume, fadeInSeconds, false);
    }

    private void StopAmbience()
    {
        StartFade(0f, fadeOutSeconds, true);
    }

    private void StartFade(float target, float seconds, bool stopAtEnd)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Fade(target, seconds, stopAtEnd));
    }

    private IEnumerator Fade(float target, float seconds, bool stopAtEnd)
    {
        float start = source.volume;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, target, t / seconds);
            yield return null;
        }
        source.volume = target;
        if (stopAtEnd) source.Stop();
        fade = null;
    }
}