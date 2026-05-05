using UnityEngine;

public class Workbench : MonoBehaviour
{
    [Header("Item Names (moeten overeenkomen met je object namen)")]
    public string stoneName = "stone";
    public string vineName = "vine";
    public string stickName = "stick";

    [Header("Result")]
    public GameObject axePrefab;

    public void TryCraft(InventorySystem inventory)
    {
        if (inventory.HasItemByName(stoneName) &&
            inventory.HasItemByName(vineName) &&
            inventory.HasItemByName(stickName))
        {
            GameObject stone = inventory.GetItemByName(stoneName);
            GameObject vine = inventory.GetItemByName(vineName);
            GameObject stick = inventory.GetItemByName(stickName);

            inventory.RemoveItem(stone);
            inventory.RemoveItem(vine);
            inventory.RemoveItem(stick);

            Instantiate(axePrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);

            Debug.Log("✅ Axe gemaakt!");
        }
        else
        {
            Debug.Log("❌ Je mist nog items!");
        }
    }
}