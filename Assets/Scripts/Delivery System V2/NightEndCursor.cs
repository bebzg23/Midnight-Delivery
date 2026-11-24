using UnityEngine;

public class NightEndCursor : MonoBehaviour
{
    [Tooltip("Player look/move scripts to disable while a panel is showing (e.g. your camera look + movement).")]
    [SerializeField] private Behaviour[] disableWhileMenuOpen;

    private DeliveryManager dm;

    private void Start()
    {
        dm = DeliveryManager.Instance;
        dm.onNightStarted.AddListener(_ => SetMenuOpen(false));
        dm.onNightCompleted.AddListener(() => SetMenuOpen(true));
        dm.onNightFailed.AddListener(() => SetMenuOpen(true));

        SetMenuOpen(false);
    }

    private void SetMenuOpen(bool open)
    {
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        foreach (var b in disableWhileMenuOpen)
            if (b != null) b.enabled = !open;
    }
}