using UnityEngine;

public class TransferBox : MonoBehaviour
{
    public Transform targetSpawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        ItemData item = other.GetComponent<ItemData>();

        if (item != null)
        {
            TransferItem(other.gameObject);
        }
    }

    void TransferItem(GameObject item)
    {
        item.transform.parent = null;

        item.transform.position = targetSpawnPoint.position + Vector3.up * 0.2f;

        Debug.Log("📦 Item getransfered!");
    }
}