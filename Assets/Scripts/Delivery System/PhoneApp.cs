using UnityEngine;
using TMPro;

// 2D overlay phone. Put this on any object (e.g. the PhoneCanvas).
// Design the phone panel in its RAISED position in the editor; the script slides it down to hide it.
//
// Waypoint rules (read by NavArrow3D):
//  1. Player is carrying a parcel      -> point at that parcel's customer (NPC), whatever is highlighted
//  2. Player is NOT carrying a parcel  -> point at the parcel highlighted on the phone (not picked up yet, or dropped)
//  3. After a delivery the highlight stays on the same list position, which is now the next parcel
public class PhoneApp : MonoBehaviour
{
    [Header("UI")]
    public RectTransform phonePanel;     // the whole phone (frame image + screen content as children)
    public TMP_Text orderListText;       // list of orders
    public TMP_Text detailText;          // details of the highlighted order

    [Header("Slide")]
    public float hideOffset = 1200f;     // how many pixels the phone drops when hidden
    public float moveSpeed = 10f;
    public bool startHidden = true;

    [Header("Phone spin (the 3D model in the studio)")]
    public Transform phoneModel;         // drag the nokia model here (optional)
    public float hiddenYaw = 0f;         // Y rotation while the phone is lowered
    public float raisedYaw = 207.1f;     // final Y rotation when the phone is raised
    public float spinSpeed = 8f;
    public float pitchX = 70f;           // X rotation the phone rests at (kept the whole time)

    [Header("Navigation")]
    public Transform waypointMarker;     // optional world object (beacon) placed on the target

    private int index = 0;
    private bool raised;
    private Vector2 raisedPos;
    private Vector2 loweredPos;
    private float currentYaw;
    private float modelZ;

    void Awake()
    {
        if (phoneModel != null)
        {
            // X comes from pitchX, Z is kept from the editor, only the Y spins
            modelZ = phoneModel.localEulerAngles.z;
        }

        if (phonePanel == null) { Debug.LogError("PhoneApp: assign Phone Panel"); enabled = false; return; }

        raisedPos = phonePanel.anchoredPosition;
        loweredPos = raisedPos - new Vector2(0f, hideOffset);
        raised = !startHidden;
        phonePanel.anchoredPosition = raised ? raisedPos : loweredPos;

        currentYaw = raised ? raisedYaw : hiddenYaw;
        ApplyYaw();
    }

    void ApplyYaw()
    {
        if (phoneModel != null)
            phoneModel.localEulerAngles = new Vector3(pitchX, currentYaw, modelZ);
    }

    void Update()
    {
        // slide up or down
        Vector2 target = raised ? raisedPos : loweredPos;
        phonePanel.anchoredPosition = Vector2.Lerp(phonePanel.anchoredPosition, target, Time.deltaTime * moveSpeed);

        // spin the 3D phone around Y (plain float lerp, so it takes the long way round)
        float targetYaw = raised ? raisedYaw : hiddenYaw;
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * spinSpeed);
        ApplyYaw();

        UpdateWaypoint();

        if (raised) Refresh();
    }

    public void Toggle() { raised = !raised; }
    public void Next() { index++; ClampIndex(); }
    public void Prev() { index--; ClampIndex(); }

    // Kept so PhoneInput still compiles. The highlighted order is now used automatically, so Enter does nothing.
    public void Select() { }

    void ClampIndex()
    {
        if (DeliveryManager.Instance == null) return;
        int n = DeliveryManager.Instance.ActiveParcels.Count;
        if (n == 0) { index = 0; return; }
        index = (index % n + n) % n;
    }

    void Refresh()
    {
        if (DeliveryManager.Instance == null) return;
        var list = DeliveryManager.Instance.ActiveParcels;
        ClampIndex();

        if (list.Count == 0)
        {
            if (orderListText) orderListText.text = "No orders";
            if (detailText) detailText.text = "";
            return;
        }

        string s = "ORDERS\n";
        for (int i = 0; i < list.Count; i++)
        {
            string mark = (i == index) ? "> " : "  ";
            string carrying = list[i].isHeld ? " [CARRYING]" : "";
            s += $"{mark}#{i + 1}  {list[i].destination.customerName}{carrying}\n";
        }
        if (orderListText) orderListText.text = s;

        Parcel p = list[index];
        string status = p.isHeld ? "Deliver to customer" : "Pick up the parcel";
        if (detailText)
            detailText.text = $"Customer: {p.destination.customerName}\nStore: {p.origin.storeName}\nStatus: {status}";
    }

    // NavArrow3D (and WaypointIndicator) read this every frame
    public bool GetWaypoint(out Transform target, out string label)
    {
        target = null;
        label = "";
        if (DeliveryManager.Instance == null) return false;

        var list = DeliveryManager.Instance.ActiveParcels;
        if (list.Count == 0) return false;

        // 1) carrying a parcel: go deliver it (ignores what is highlighted)
        for (int i = 0; i < list.Count; i++)
        {
            Parcel p = list[i];
            if (p != null && p.isHeld)
            {
                target = p.destination.transform;
                label = p.destination.customerName;
                return true;
            }
        }

        // 2) not carrying anything: point at the parcel highlighted on the phone
        ClampIndex();
        Parcel highlighted = list[index];
        if (highlighted == null) return false;

        target = highlighted.transform;
        label = $"Parcel for {highlighted.destination.customerName}";
        return true;
    }

    void UpdateWaypoint()
    {
        // optional: a plain world object (beacon) placed above the target
        if (waypointMarker == null) return;

        if (!GetWaypoint(out Transform target, out string label))
        {
            waypointMarker.gameObject.SetActive(false);
            return;
        }

        waypointMarker.gameObject.SetActive(true);
        waypointMarker.position = target.position + Vector3.up * 3f;
    }
}