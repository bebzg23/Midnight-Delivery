using System;
using UnityEngine;

/// Put this on your parcel prefab (same object as ObjectGrabbable).
public class Parcel : MonoBehaviour
{
    /// Fired whenever a parcel is picked up (true) or dropped (false). Used by the arrow.
    public static event Action<Parcel, bool> OnHeldChanged;

    public DeliveryOrder Order { get; private set; }
    public ParcelDefinition Definition { get; private set; }

    /// True the first time the player picks it up.
    public bool PickedUp { get; set; }

    bool isHeld;
    public bool IsHeld
    {
        get => isHeld;
        set
        {
            if (isHeld == value) return;
            isHeld = value;
            OnHeldChanged?.Invoke(this, value);
        }
    }

    public void Init(DeliveryOrder order)
    {
        Order = order;
        Definition = order.Item;
        gameObject.name = $"Parcel_{order.Item.displayName}_to_{order.Customer.npcName}";
    }
}