using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ControllerMusicUI : MonoBehaviour
{
    public GameObject musicMenu;

    void Update()
    {
        var device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        bool pressed;
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out pressed) && pressed)
        {
            musicMenu.SetActive(!musicMenu.activeSelf);
        }
    }
}