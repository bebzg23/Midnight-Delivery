using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerPickupDrop : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask pickUpLayerMask;
    [SerializeField] private float pickUpRadius = 2f;
    [SerializeField, Range(0f, 180f)] private float pickUpAngle = 70f; // half-angle in front of player
    [SerializeField] private Transform objectGrabPointTransform;       // child of the player, in front of them

    [SerializeField] private GameObject pickUpPrompt;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 0.75f, 0f);

    private readonly Collider[] results = new Collider[16];
    private ObjectGrabbable held;
    private ObjectGrabbable candidate;
    private Collider[] playerColliders;   // NEW: all colliders on the player

    private void Awake()
    {
        // NEW: cache the player's colliders so held objects can ignore them
        if (player != null)
            playerColliders = player.GetComponentsInChildren<Collider>();
        else
            playerColliders = GetComponentsInChildren<Collider>();
    }

    void Update()
    {
        bool ePressed = Keyboard.current.eKey.wasPressedThisFrame;

        if (held != null)
        {
            if (ePressed)
            {
                SetParcelHeld(held, false);   // tell the delivery system it was dropped
                held.Drop();
                held = null;
            }
            return;
        }

        candidate = FindBestCandidate();
        UpdatePrompt();

        if (candidate != null && ePressed)
        {
            candidate.Grab(objectGrabPointTransform, playerColliders);   // CHANGED: passes player colliders
            held = candidate;
            SetParcelHeld(held, true);        // tell the delivery system it was picked up
            candidate = null;
            pickUpPrompt.SetActive(false);
        }
    }

    // marks the Parcel (if this object is one) as held / dropped
    private void SetParcelHeld(ObjectGrabbable obj, bool isHeld)
    {
        if (obj == null) return;

        Parcel parcel = obj.GetComponentInParent<Parcel>();
        if (parcel == null) return;

        parcel.IsHeld = isHeld;
        if (isHeld) parcel.PickedUp = true;
    }

    private void UpdatePrompt()
    {
        if (candidate == null)
        {
            pickUpPrompt.SetActive(false);
            return;
        }

        pickUpPrompt.transform.position = candidate.transform.position + promptOffset;
        pickUpPrompt.SetActive(true);
    }

    private ObjectGrabbable FindBestCandidate()
    {
        int count = Physics.OverlapSphereNonAlloc(player.position, pickUpRadius, results, pickUpLayerMask);

        ObjectGrabbable best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (!results[i].TryGetComponent(out ObjectGrabbable grabbable)) continue;

            Vector3 toObject = results[i].bounds.center - player.position;
            toObject.y = 0f;

            if (Vector3.Angle(player.forward, toObject) > pickUpAngle) continue;

            // lower = better: closer and more centered wins
            float score = toObject.magnitude + Vector3.Angle(player.forward, toObject) * 0.02f;
            if (score < bestScore)
            {
                bestScore = score;
                best = grabbable;
            }
        }

        return best;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, pickUpRadius);
    }
}