using UnityEngine;

public class Jumpscare : MonoBehaviour
{
    public GameObject uiCanvas; // Assign your UI panel or canvas in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            uiCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Optional: Close the UI when the player leaves the zone
        if (other.CompareTag("Player"))
        {
            uiCanvas.SetActive(false);
        }
    }
}
