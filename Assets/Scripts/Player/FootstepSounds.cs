using UnityEngine;

/// Put on the player. Plays a footstep every time the player has moved 'stepDistance' metres
/// along the ground. Works with CharacterController, Rigidbody or any other mover, because it
/// only looks at how far the object actually moved. Running = faster steps automatically.
[RequireComponent(typeof(AudioSource))]
public class FootstepSounds : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Timing")]
    [Tooltip("Metres travelled between steps. Smaller = faster steps.")]
    [SerializeField] private float stepDistance = 1.6f;
    [Tooltip("Below this speed (m/s) no steps play, so tiny drifts stay silent.")]
    [SerializeField] private float minSpeed = 0.5f;

    [Header("Sound")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [Header("Ground check")]
    [Tooltip("How far below the player's pivot to look for ground. Pivot at the feet: ~0.3. Pivot at the centre of a 2 m capsule: ~1.2.")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask = ~0;

    private AudioSource source;
    private Vector3 lastPos;
    private float distanceAccum;
    private int lastIndex = -1;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D: always heard the same, since it's the player's own feet
    }

    private void Start()
    {
        lastPos = transform.position;
    }

    private void Update()
    {
        Vector3 delta = transform.position - lastPos;
        lastPos = transform.position;
        delta.y = 0f;

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float speed = delta.magnitude / dt;

        if (!IsGrounded() || speed < minSpeed)
        {
            // keep a little of the progress so the first step after stopping comes quickly
            distanceAccum = Mathf.Min(distanceAccum, stepDistance * 0.5f);
            return;
        }

        distanceAccum += delta.magnitude;
        if (distanceAccum >= stepDistance)
        {
            distanceAccum = 0f;
            PlayStep();
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down,
            groundCheckDistance + 0.1f, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void PlayStep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        // pick a random clip, but never the same one twice in a row
        int index = Random.Range(0, footstepClips.Length);
        if (footstepClips.Length > 1 && index == lastIndex)
            index = (index + 1) % footstepClips.Length;
        lastIndex = index;

        var clip = footstepClips[index];
        if (clip == null) return;

        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, volume);
    }
}