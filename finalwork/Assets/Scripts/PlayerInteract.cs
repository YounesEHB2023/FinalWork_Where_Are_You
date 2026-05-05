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

            // Raycast recht vooruit (werkt met controller + keyboard)
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log("🎯 Hit: " + hit.collider.name);

                Workbench bench = hit.collider.GetComponent<Workbench>();

                if (bench != null)
                {
                    Debug.Log("✅ Workbench geraakt!");

                    InventorySystem inv = GetComponentInChildren<InventorySystem>();

                    if (inv != null)
                    {
                        bench.TryCraft(inv);
                    }
                    else
                    {
                        Debug.LogError("❌ Geen InventorySystem op Player!");
                    }
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