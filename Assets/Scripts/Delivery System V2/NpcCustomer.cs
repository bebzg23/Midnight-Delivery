using UnityEngine;

/// Put on each NPC. Add a Collider (Is Trigger = ON) around where the NPC receives parcels.
public class NpcCustomer : MonoBehaviour
{
    public string npcName = "NPC";
    [Tooltip("Optional: an icon/bubble shown while this NPC is waiting for a parcel.")]
    public GameObject orderBubble;

    public DeliveryOrder ActiveOrder { get; private set; }

    void Awake() { SetBubble(false); }

    public void AssignOrder(DeliveryOrder order)
    {
        ActiveOrder = order;
        SetBubble(true);
    }

    public void ClearOrder()
    {
        ActiveOrder = null;
        SetBubble(false);
    }

    void SetBubble(bool on) { if (orderBubble) orderBubble.SetActive(on); }

    // Parcel dropped (not held) inside the trigger = delivered.
    // Stay is used so dropping the parcel while already standing in the zone still counts.
    void OnTriggerStay(Collider other)
    {
        if (ActiveOrder == null) return;

        var parcel = other.GetComponentInParent<Parcel>();
        if (parcel == null || parcel.IsHeld) return;
        if (parcel.Order != ActiveOrder) return; // wrong parcel -> ignored

        DeliveryManager.Instance.Deliver(ActiveOrder);
    }
}
