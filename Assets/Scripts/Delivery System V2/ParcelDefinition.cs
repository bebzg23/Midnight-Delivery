using UnityEngine;

[CreateAssetMenu(menuName = "Delivery/Parcel Definition", fileName = "Parcel_")]
public class ParcelDefinition : ScriptableObject
{
    public string displayName = "Small Box";
    [Tooltip("Your existing parcel prefab (needs Rigidbody + Collider + Parcel component).")]
    public GameObject prefab;

    [Header("Rewards")]
    public int minReward = 10;
    public int maxReward = 20;
}