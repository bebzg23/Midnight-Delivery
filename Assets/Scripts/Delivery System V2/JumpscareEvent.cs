using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering; // HDRP Volume

/// Put on an object in the scene (e.g. "Scare_Window", "Scare_Hallway").
/// Add visuals/animations/lights through the UnityEvent in the Inspector.
public class JumpscareEvent : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource sfx;

    [Header("Optional HDRP screen effect")]
    [Tooltip("A Global Volume with e.g. Chromatic Aberration / Vignette, weight 0 by default.")]
    public Volume pulseVolume;
    public float pulseDuration = 0.5f;

    [Header("Custom stuff (activate monster, flicker lights, play animation...)")]
    public UnityEvent onTriggered;

    public void Trigger()
    {
        if (sfx) sfx.Play();
        onTriggered.Invoke();
        if (pulseVolume) StartCoroutine(Pulse());
    }

    IEnumerator Pulse()
    {
        float t = 0f;
        while (t < pulseDuration)
        {
            t += Time.deltaTime;
            pulseVolume.weight = Mathf.Sin(t / pulseDuration * Mathf.PI); // 0 -> 1 -> 0
            yield return null;
        }
        pulseVolume.weight = 0f;
    }
}
