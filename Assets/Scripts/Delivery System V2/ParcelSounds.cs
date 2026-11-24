using UnityEngine;

/// One object in the scene. Plays a sound whenever any parcel is picked up or dropped.
/// Works with every parcel prefab, no per-prefab setup needed.
public class ParcelSounds : MonoBehaviour
{
    [Header("Clips (one is picked at random if you add several)")]
    [SerializeField] private AudioClip[] pickupClips;
    [SerializeField] private AudioClip[] dropClips;

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
    [Tooltip("Random pitch range so repeats don't sound identical.")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [Tooltip("Ignore events closer together than this (stops double-triggers).")]
    [SerializeField] private float minInterval = 0.1f;

    private AudioSource source;
    private float lastPlayTime = -10f;

    private void Awake()
    {
        // Small 2D source used to play the one-shots.
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private void OnEnable()  { Parcel.OnHeldChanged += HandleHeldChanged; }
    private void OnDisable() { Parcel.OnHeldChanged -= HandleHeldChanged; }

    private void HandleHeldChanged(Parcel parcel, bool held)
    {
        if (Time.time - lastPlayTime < minInterval) return;

        var clips = held ? pickupClips : dropClips;
        if (clips == null || clips.Length == 0) return;

        var clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        lastPlayTime = Time.time;
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, volume);
    }
}