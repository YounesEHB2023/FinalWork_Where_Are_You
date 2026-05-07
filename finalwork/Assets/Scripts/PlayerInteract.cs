using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float range = 5f;
    public Camera playerCamera;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("🟡 E pressed");

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log("🎯 Hit: " + hit.collider.name);

                WorkbenchCraft bench = hit.collider.GetComponent<WorkbenchCraft>();

                if (bench != null)
                {
                    Debug.Log("✅ Workbench geraakt!");
                }
                else
                {
                    Debug.Log("❌ Geen Workbench op dit object");
                }
            }
            else
            {
                Debug.Log("❌ Raycast raakt niets");
            }
        }
    }
}