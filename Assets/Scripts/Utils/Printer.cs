using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Printer : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.f6Key.wasReleasedThisFrame)
            ScreenCapture.CaptureScreenshot($"Prints/{Guid.NewGuid().ToString()}.png");
    }
}
