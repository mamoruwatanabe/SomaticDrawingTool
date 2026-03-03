using UnityEngine;
using Unity.XR.CoreUtils;

public class XRViewSwitcher : MonoBehaviour
{
    [Header("XR Origin (drag XR Origin here)")]
    public XROrigin xrOrigin;

    [Header("Assign View_A, View_B, View_C... here (scene-root objects)")]
    public Transform[] views;

    [Header("Behaviour")]
    [Tooltip("If true, keep the rig's current Y (height) when switching.")]
    public bool preserveHeight = true;

    [Tooltip("If true, only apply target Y rotation (yaw). Recommended for comfort.")]
    public bool yawOnly = true;

    [Tooltip("Start on this view index.")]
    public int startIndex = 0;

    int currentIndex = 0;

    void Awake()
    {
        if (xrOrigin == null)
            xrOrigin = GetComponent<XROrigin>();
    }

    void Start()
    {
        if (!IsReady())
        {
            Debug.LogWarning("XRViewSwitcher: xrOrigin or views not set.");
            return;
        }

        currentIndex = Mathf.Clamp(startIndex, 0, views.Length - 1);
        ApplyView(currentIndex);
    }

    public void NextView()
    {
        if (!IsReady()) return;

        currentIndex = (currentIndex + 1) % views.Length;
        ApplyView(currentIndex);
    }

    public void PreviousView()
    {
        if (!IsReady()) return;

        currentIndex = (currentIndex - 1 + views.Length) % views.Length;
        ApplyView(currentIndex);
    }

    public void ToggleAB()
    {
        if (views == null || views.Length < 2)
        {
            Debug.LogWarning("XRViewSwitcher: Need at least 2 views for ToggleAB().");
            return;
        }

        currentIndex = (currentIndex == 0) ? 1 : 0;
        ApplyView(currentIndex);
    }

    public void GoToView(int index)
    {
        if (!IsReady()) return;

        currentIndex = Mathf.Clamp(index, 0, views.Length - 1);
        ApplyView(currentIndex);
    }

    bool IsReady()
    {
        if (xrOrigin == null) return false;
        if (views == null || views.Length == 0) return false;
        for (int i = 0; i < views.Length; i++)
            if (views[i] == null) return false;
        return true;
    }

    void ApplyView(int index)
{
    Transform target = views[index];

    // Move XR Origin XZ directly to the target - no head tracking compensation.
    // Head tracking X/Z offset is physical and should not affect teleport destination.
    Vector3 newPos = target.position;

    // Only adjust Y so the camera (eyes) lands at the view's Y position,
    // accounting for the static camera floor offset.
    float cameraFloorOffset = xrOrigin.Camera.transform.position.y - xrOrigin.transform.position.y;
    newPos.y = target.position.y - cameraFloorOffset;

    if (preserveHeight)
        newPos.y = xrOrigin.transform.position.y;

    Quaternion newRot = yawOnly
        ? Quaternion.Euler(0f, target.eulerAngles.y, 0f)
        : target.rotation;

    xrOrigin.transform.SetPositionAndRotation(newPos, newRot);

    Debug.Log($"XRViewSwitcher: Switched to view {index} ({target.name})");
}

}
