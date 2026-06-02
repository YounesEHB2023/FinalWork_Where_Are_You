using UnityEngine;

public class PickupGlow : MonoBehaviour
{
    [Header("Proximity Settings")]
public float proximityDistance = 3f;

    public Color proximityColor = Color.white;
    public float proximityIntensity = 0.6f;

    public Color focusColor = Color.white;
    public float focusIntensity = 1.5f;

    private Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        SetGlowOff();
    }

    public void SetProximityGlow()
    {
        ApplyGlow(proximityColor, proximityIntensity);
    }

    public void SetFocusGlow()
    {
        ApplyGlow(focusColor, focusIntensity);
    }

    public void SetGlowOff()
    {
        ApplyGlow(Color.black, 0f);
    }

    void ApplyGlow(Color color, float intensity)
    {
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * intensity);
            }
        }
    }
}