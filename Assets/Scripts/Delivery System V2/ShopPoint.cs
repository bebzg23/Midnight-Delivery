using UnityEngine;

/// Put on each shop. Parcels for orders spawn near spawnPoint.
public class ShopPoint : MonoBehaviour
{
    public string shopName = "Shop";
    [Tooltip("Empty child placed on the counter / shelf. Falls back to this transform.")]
    public Transform spawnPoint;
    public float scatterRadius = 0.3f;

    public Vector3 GetSpawnPosition(System.Random rng)
    {
        Vector3 basePos = spawnPoint ? spawnPoint.position : transform.position;
        float x = (float)(rng.NextDouble() * 2 - 1) * scatterRadius;
        float z = (float)(rng.NextDouble() * 2 - 1) * scatterRadius;
        return basePos + new Vector3(x, 0.1f, z);
    }
}
