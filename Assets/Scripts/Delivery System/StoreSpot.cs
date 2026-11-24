using UnityEngine;

// Put this on each of your 3 store locations (an empty GameObject placed in the map).
public class StoreSpot : MonoBehaviour
{
    public string storeName = "Store";

    // Where parcels appear. If left empty, the store's own transform is used.
    public Transform spawnPoint;

    public Vector3 SpawnPosition(int index)
    {
        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;
        // spread multiple parcels out a little so they don't overlap
        return basePos + new Vector3(index * 0.6f, 0.2f, 0f);
    }
}