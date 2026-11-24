using UnityEngine;

/// One object in the scene. Plays:
///  - a coin sound every time a parcel is delivered,
///  - a congratulatory sound when the whole night is completed,
///  - (optional) a fail sound when time runs out.
[RequireComponent(typeof(AudioSource))]
public class DeliverySounds : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip nightCompleteClip;
    [SerializeField] private AudioClip nightFailedClip;

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float coinVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float nightCompleteVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float nightFailedVolume = 0.9f;
    [Tooltip("Delay the congrats sound so it doesn't overlap the last coin sound.")]
    [SerializeField] private float nightCompleteDelay = 0.6f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D
    }

    private void Start()
    {
        var dm = DeliveryManager.Instance;
        dm.onOrderDelivered.AddListener(OnOrderDelivered);
        dm.onNightCompleted.AddListener(OnNightCompleted);
        dm.onNightFailed.AddListener(OnNightFailed);
    }

    private void OnDestroy()
    {
        var dm = DeliveryManager.Instance;
        if (dm == null) return;
        dm.onOrderDelivered.RemoveListener(OnOrderDelivered);
        dm.onNightCompleted.RemoveListener(OnNightCompleted);
        dm.onNightFailed.RemoveListener(OnNightFailed);
    }

    private void OnOrderDelivered(DeliveryOrder order)
    {
        if (coinClip) source.PlayOneShot(coinClip, coinVolume);
    }

    private void OnNightCompleted()
    {
        if (nightCompleteClip == null) return;
        Invoke(nameof(PlayComplete), nightCompleteDelay);
    }

    private void PlayComplete()
    {
        source.PlayOneShot(nightCompleteClip, nightCompleteVolume);
    }

    private void OnNightFailed()
    {
        if (nightFailedClip) source.PlayOneShot(nightFailedClip, nightFailedVolume);
    }
}