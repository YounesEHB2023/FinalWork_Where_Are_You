using UnityEngine;

public class OutlineProximity : MonoBehaviour
{
    public float activationDistance = 3f;

    private Outline outline;
    private Transform localPlayerCamera;

    void Start()
    {
        outline = GetComponent<Outline>();

        if (outline != null)
            outline.enabled = false;

        FindLocalPlayerCamera();
    }

    void Update()
    {
        if (outline == null) return;

        if (localPlayerCamera == null)
            FindLocalPlayerCamera();

        if (localPlayerCamera == null)
        {
            outline.enabled = false;
            return;
        }

        float distance = Vector3.Distance(localPlayerCamera.position, transform.position);
        outline.enabled = distance <= activationDistance;
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