using UnityEngine;

// 3D navigation arrow (NFS style). It floats in front of the camera and always points at the
// store / NPC of the order chosen on the phone.
//
// Setup: Main Camera > ArrowPivot (empty, THIS script) > ArrowModel (your 3D arrow).
// The model's tip must point along the pivot's forward (blue Z) axis.
public class NavArrow3D : MonoBehaviour
{
    [Header("References")]
    public PhoneApp phone;           // the PhoneApp in the scene (gives us the current target)
    public GameObject visual;        // the arrow model (child of this object). Hidden when there is no target
    public Transform viewer;         // what the arrow is measured from. Empty = Main Camera

    [Header("Behaviour")]
    public float turnSpeed = 10f;    // higher = snappier
    public float hideWithin = 4f;    // hide when closer than this many meters (0 = never hide)
    public float extraPitch = 15f;   // tilts the nose down (+) or up (-) so the arrow doesn't look flat

    [Header("Bobbing (optional)")]
    public float bobAmount = 0.01f;  // 0 = off
    public float bobSpeed = 3f;

    private Vector3 baseLocalPos;
    private bool initialized;

    void Start()
    {
        baseLocalPos = transform.localPosition;
        if (viewer == null && Camera.main != null) viewer = Camera.main.transform;
        if (visual == null) Debug.LogError("NavArrow3D: assign the arrow model to Visual (it must be a child, not this object)");
    }

    void LateUpdate()
    {
        if (phone == null || visual == null) return;
        if (viewer == null)
        {
            if (Camera.main == null) return;
            viewer = Camera.main.transform;
        }

        if (!phone.GetWaypoint(out Transform target, out string targetName))
        {
            visual.SetActive(false);
            return;
        }

        // flat direction from the camera to the target
        Vector3 to = target.position - viewer.position;
        to.y = 0f;

        if (to.sqrMagnitude < 0.0001f || (hideWithin > 0f && to.magnitude < hideWithin))
        {
            visual.SetActive(false);
            return;
        }
        visual.SetActive(true);

        // rotate in WORLD space, so the arrow keeps pointing at the target while the camera turns
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up) * Quaternion.Euler(extraPitch, 0f, 0f);

        if (!initialized)
        {
            transform.rotation = look;   // snap on the first frame
            initialized = true;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
        }

        if (bobAmount > 0f)
            transform.localPosition = baseLocalPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmount);
    }
}