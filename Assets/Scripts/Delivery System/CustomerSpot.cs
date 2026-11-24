using UnityEngine;

// Put this on each of your 3 NPCs. Add a trigger Collider (Is Trigger = true) around the NPC.
public class CustomerSpot : MonoBehaviour
{
    public string customerName = "Customer";

    void OnTriggerEnter(Collider other)
    {
        // Your parcel prefabs need the "Parcel" component below
        Parcel parcel = other.GetComponent<Parcel>();
        if (parcel == null) return;

        // Only accept the parcel if it was ordered by THIS npc
        if (parcel.destination == this)
        {
            Debug.Log($"Correct! Parcel from {parcel.origin.storeName} delivered to {customerName}");
            DeliveryManager.Instance.ParcelDelivered(parcel);
        }
        else
        {
            Debug.LogWarning($"Wrong parcel! {customerName} did not order this. It belongs to {parcel.destination.customerName} (from {parcel.origin.storeName})");
        }
    }
}