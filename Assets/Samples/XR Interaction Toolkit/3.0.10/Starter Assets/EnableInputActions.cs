using UnityEngine;
using UnityEngine.InputSystem;

public class EnableInputActions : MonoBehaviour
{
    public InputActionAsset inputActions;

    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            Debug.Log("Input Actions Enabled!");
        }
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }
    }
}
