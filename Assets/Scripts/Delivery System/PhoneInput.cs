using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneInput : MonoBehaviour
{
    public PhoneApp phone;

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame) phone.Toggle();

        // only navigate while the phone is up
        if (Keyboard.current.upArrowKey.wasPressedThisFrame) phone.Prev();
        if (Keyboard.current.downArrowKey.wasPressedThisFrame) phone.Next();
        if (Keyboard.current.enterKey.wasPressedThisFrame) phone.Select();
    }
}