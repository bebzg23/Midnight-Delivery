using UnityEngine;

/// Rotates an arrow model to point at:
///  1) the customer of the parcel you are holding (even if it is not the top order),
///  2) otherwise the last parcel you picked up (after dropping it),
///  3) otherwise the parcel of the first undelivered order in the list.
public class DeliveryArrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [Tooltip("The arrow model that rotates. Leave empty to rotate this object.")]
    [SerializeField] private Transform arrowModel;
    [Tooltip("Only needed if the arrow is rendered by a separate camera (render texture / overlay camera).")]
    [SerializeField] private Camera viewCamera;

    [Header("Behaviour")]
    [SerializeField] private float turnSpeed = 12f;
    [Tooltip("Ignore height difference, like a compass.")]
    [SerializeField] private bool flattenToHorizontal = true;
    [Tooltip("Fix for models whose forward axis isn't Z (e.g. 0,90,0 or 90,0,0).")]
    [SerializeField] private Vector3 modelRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 customerOffset = new Vector3(0f, 1.5f, 0f);

    private Parcel heldParcel;
    private Parcel lastPickedUp;
    private DeliveryManager dm;

    private void Awake()
    {
        if (arrowModel == null) arrowModel = transform;
    }

    private void OnEnable()  { Parcel.OnHeldChanged += HandleHeldChanged; }
    private void OnDisable() { Parcel.OnHeldChanged -= HandleHeldChanged; }

    private void Start()
    {
        dm = DeliveryManager.Instance;
        dm.onNightStarted.AddListener(_ => { heldParcel = null; lastPickedUp = null; });
    }

    private void HandleHeldChanged(Parcel parcel, bool held)
    {
        if (held)
        {
            heldParcel = parcel;
            lastPickedUp = parcel;
        }
        else if (heldParcel == parcel)
        {
            heldParcel = null; // lastPickedUp stays, so the arrow points back at it
        }
    }

    private void Update()
    {
        if (dm == null) return;

        Vector3 targetPos = default;
        bool hasTarget = dm.NightActive && TryGetTarget(out targetPos);
        SetArrowVisible(hasTarget);
        if (!hasTarget) return;

        PointAt(targetPos);
    }

    // If the arrow model IS this object, deactivating it would also stop this script (and its
    // event subscriptions), so the arrow would never come back. Toggle renderers in that case.
    private void SetArrowVisible(bool visible)
    {
        if (arrowModel.gameObject != gameObject)
        {
            arrowModel.gameObject.SetActive(visible);
            return;
        }
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
    }

    private bool TryGetTarget(out Vector3 pos)
    {
        pos = default;

        // 1) Holding a parcel -> its customer
        if (heldParcel != null && heldParcel.Order != null && !heldParcel.Order.Delivered)
        {
            pos = heldParcel.Order.Customer.transform.position + customerOffset;
            return true;
        }

        // 2) Dropped it -> point back at the last parcel picked up
        if (lastPickedUp != null && lastPickedUp.Order != null && !lastPickedUp.Order.Delivered)
        {
            pos = lastPickedUp.transform.position;
            return true;
        }

        // 3) Top undelivered order in the list -> its parcel
        foreach (var order in dm.Orders)
        {
            if (order.Delivered) continue;

            pos = order.Instance != null
                ? order.Instance.transform.position
                : order.Customer.transform.position + customerOffset;
            return true;
        }

        return false;
    }

    private void PointAt(Vector3 targetPos)
    {
        Vector3 origin = player != null ? player.position : arrowModel.position;
        Vector3 dir = targetPos - origin;
        if (flattenToHorizontal) dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion desired;
        if (viewCamera != null && Camera.main != null)
        {
            // express the world direction in the main camera's space, then show it in the view camera's space
            Vector3 local = Camera.main.transform.InverseTransformDirection(dir);
            desired = viewCamera.transform.rotation * Quaternion.LookRotation(local);
        }
        else
        {
            desired = Quaternion.LookRotation(dir);
        }

        desired *= Quaternion.Euler(modelRotationOffset);
        arrowModel.rotation = Quaternion.Slerp(
            arrowModel.rotation, desired, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
    }
}