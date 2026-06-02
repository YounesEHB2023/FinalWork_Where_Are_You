using UnityEngine;

public class LocalMultiplayerDisplayManager : MonoBehaviour
{
    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Debug.Log("Display 2 activated");
        }
        else
        {
            Debug.LogWarning("No second display found. HDMI screen not detected.");
        }
    }
}