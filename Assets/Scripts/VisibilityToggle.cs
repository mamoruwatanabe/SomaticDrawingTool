using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VisibilityToggle : MonoBehaviour
{
    public GameObject targetObject; // Object to show/hide

    private InputDevice leftController;
    private bool lastButtonState = false;

    void Start()
    {
        // Get the right-hand controller
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    void Update()
    {
        // Re-acquire device if lost
        if (!leftController.isValid)
        {
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        // B button = secondaryButton on right controller
        bool buttonPressed;
        if (leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonPressed))
        {
            // Detect button DOWN (not held)
            if (buttonPressed && !lastButtonState)
            {
                ToggleVisibility();
            }

            lastButtonState = buttonPressed;
        }
    }

    void ToggleVisibility()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
