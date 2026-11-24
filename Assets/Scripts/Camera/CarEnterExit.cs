using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Unity.Cinemachine;
public class CarEnterExit : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject player;              
    [SerializeField] MonoBehaviour carController;
    [SerializeField] Transform exitPoint;

    [Header("Settings")]
    [SerializeField] float enterDistance = 3f;

    bool driving;
    CharacterController playerCC;

    [SerializeField] CinemachineCamera freeLook;
    [SerializeField] Transform playerCamTarget;
    [SerializeField] Transform carCamTarget; 

    [SerializeField] private GameObject drivingPrompt;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 0.75f, 0f);

    

    void Start()
    {
        if (player != null) playerCC = player.GetComponent<CharacterController>();
        SetDriving(false);
    }

    void Update()
    {
        UpdatePrompt();

        if (!InteractPressed()) return;

        if (driving)
            SetDriving(false);
        else if (player != null && Vector3.Distance(player.transform.position, transform.position) <= enterDistance)
            SetDriving(true);
    }

    private void UpdatePrompt()
    {
        if (drivingPrompt == null) return;

        bool inRange = !driving
                    && player != null
                    && Vector3.Distance(player.transform.position, transform.position) <= enterDistance;

        if (inRange)
            drivingPrompt.transform.position = transform.position + promptOffset;

        drivingPrompt.SetActive(inRange);
    }

    void SetDriving(bool value)
    {
        driving = value;

        if (value)
        {
            player.SetActive(false);
        }
        else
        {
            // Move the player BEFORE re-activating. A CharacterController overrides
            // transform.position unless it's disabled while you teleport.
            if (playerCC != null) playerCC.enabled = false;
            player.transform.SetPositionAndRotation(exitPoint.position, exitPoint.rotation);
            player.SetActive(true);
            if (playerCC != null) playerCC.enabled = true;
        }

        if (carController != null) carController.enabled = value;

        if (freeLook != null)
        {
            Transform t = value ? carCamTarget : playerCamTarget;
            freeLook.Follow = t;
            freeLook.LookAt = t;
        }
    }

    bool InteractPressed()
    {
    #if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
    #else
            return Input.GetKeyDown(KeyCode.F);
    #endif
    }
}