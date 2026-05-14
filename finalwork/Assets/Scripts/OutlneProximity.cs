using UnityEngine;

public class OutlineProximity : MonoBehaviour
{
    public float activationDistance = 3f;

    private Outline outline;
    private Transform localPlayerCamera;
    private bool initialized = false;

    void Start()
    {
        outline = GetComponent<Outline>();
        FindLocalPlayerCamera();

        if (outline != null)
        {
            outline.enabled = true;
            outline.OutlineWidth = 0f;
        }

        initialized = true;
    }

    void Update()
    {
        if (!initialized || outline == null) return;

        if (localPlayerCamera == null)
            FindLocalPlayerCamera();

        if (localPlayerCamera == null)
        {
            outline.OutlineWidth = 0f;
            return;
        }

        float distance = Vector3.Distance(localPlayerCamera.position, transform.position);

        outline.OutlineWidth = distance <= activationDistance ? 5f : 0f;
    }

    void FindLocalPlayerCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera cam in cameras)
        {
            if (cam.enabled)
            {
                localPlayerCamera = cam.transform;
                return;
            }
        }
    }
}