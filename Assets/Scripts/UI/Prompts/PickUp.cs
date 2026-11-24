using UnityEngine;

public class PickUp : MonoBehaviour
{
    private Transform cam;

    void Start() => cam = Camera.main.transform;

    void LateUpdate() => transform.forward = cam.forward;
}
