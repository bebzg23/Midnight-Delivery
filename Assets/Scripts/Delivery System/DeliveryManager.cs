using System.Collections.Generic;
using UnityEngine;

// Add this to one empty GameObject called "DeliveryManager".
// Attach this Parcel script to your 3 parcel prefabs too (same file is fine).
public class Parcel : MonoBehaviour
{
    [HideInInspector] public CustomerSpot destination;
    [HideInInspector] public StoreSpot origin;
    [HideInInspector] public bool pickedUp;   // set to true in your pickup code
    [HideInInspector] public Vector3 spawnPosition;

    [Header("Fallback pickup detection")]
    public bool autoDetectPickup = true;      // marks pickedUp when the parcel is moved away from the store
    public float pickupDistance = 1.5f;

    [Header("Held / dropped detection")]
    public bool autoDetectHeld = false;       // OFF: PlayerPickupDrop sets isHeld. Turn on only if you don't use that script (see the two options below)
    public bool heldIfParented = true;        // held = the parcel is a child of something (hand, car trunk slot...)
    public bool heldIfKinematic = false;      // held = its Rigidbody is kinematic (turn on if your pickup does this and does not parent)

    // true while the player carries it (or it is inside the car). If autoDetectHeld is off, set it from your pickup/drop code.
    [HideInInspector] public bool isHeld;

    private Rigidbody rb;

    // Picked up at some point, but now lying somewhere (dropped)
    public bool IsDropped => pickedUp && !isHeld;

    void Awake() { rb = GetComponent<Rigidbody>(); }

    void Update()
    {
        if (autoDetectHeld)
        {
            isHeld = (heldIfParented && transform.parent != null) ||
                     (heldIfKinematic && rb != null && rb.isKinematic);

            if (isHeld && !pickedUp)
            {
                pickedUp = true;
                Debug.Log($"Parcel for {destination.customerName} picked up");
            }
        }

        if (autoDetectPickup && !pickedUp &&
            Vector3.Distance(transform.position, spawnPosition) > pickupDistance)
        {
            pickedUp = true;
            Debug.Log($"Parcel for {destination.customerName} picked up");
        }
    }
}

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    [Header("Scene setup")]
    public StoreSpot[] stores;            // drag your 3 stores here
    public CustomerSpot[] customers;      // drag your 3 NPCs here
    public GameObject[] parcelPrefabs;    // drag your 3 parcel objects here

    [Header("Order settings")]
    public int minParcels = 2;
    public int maxParcels = 4;            // total parcels per round

    private List<Parcel> activeParcels = new List<Parcel>();
    public IReadOnlyList<Parcel> ActiveParcels => activeParcels;   // the phone app reads this

    void Awake() { Instance = this; }

    void Start() { GenerateOrders(); }

    public void GenerateOrders()
    {
        // clear leftovers
        foreach (var p in activeParcels) if (p != null) Destroy(p.gameObject);
        activeParcels.Clear();

        int total = Random.Range(minParcels, maxParcels + 1);

        // count how many parcels already placed at each store, for spacing
        int[] storeCount = new int[stores.Length];

        for (int i = 0; i < total; i++)
        {
            // each parcel picks a random store, random customer, random prefab
            int s = Random.Range(0, stores.Length);
            int c = Random.Range(0, customers.Length);
            int prefab = Random.Range(0, parcelPrefabs.Length);

            Vector3 pos = stores[s].SpawnPosition(storeCount[s]++);
            GameObject obj = Instantiate(parcelPrefabs[prefab], pos, Quaternion.identity);

            Parcel parcel = obj.GetComponent<Parcel>();
            if (parcel == null) parcel = obj.AddComponent<Parcel>();
            parcel.spawnPosition = pos;
            parcel.origin = stores[s];
            parcel.destination = customers[c];

            activeParcels.Add(parcel);
            Debug.Log($"Order {i + 1}: pick up at {stores[s].storeName} -> deliver to {customers[c].customerName}");
        }
    }

    public void ParcelDelivered(Parcel parcel)
    {
        if (!activeParcels.Contains(parcel)) return;
        activeParcels.Remove(parcel);
        Destroy(parcel.gameObject);
        Debug.Log("Delivered! Remaining: " + activeParcels.Count);

        if (activeParcels.Count == 0)
        {
            Debug.Log("All delivered, generating new orders");
            GenerateOrders();
        }
    }
}