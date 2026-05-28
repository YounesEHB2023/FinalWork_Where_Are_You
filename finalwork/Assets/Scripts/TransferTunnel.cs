using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransferTunnel : MonoBehaviour
{
    public Transform visualStartPoint;
    public Transform inPoint;
    public Transform targetSpawnPoint;

    public GameObject pressXUIPlayer1;
    public GameObject pressXUIPlayer2;

    private InventorySystem[] inventories = new InventorySystem[2];
    private PickupSystem[] pickups = new PickupSystem[2];
    private bool[] playersInside = new bool[2];

    private bool isTransferring;

    void Start()
    {
        HideUI();
    }

    void Update()
    {
        UpdateUI();

        for (int i = 0; i < 2; i++)
        {
            if (!playersInside[i]) continue;
            if (inventories[i] == null || pickups[i] == null) continue;
            if (isTransferring) continue;

            GameObject selectedItem = inventories[i].GetSelectedItem();
            if (selectedItem == null) continue;

            Gamepad pad = GetAssignedGamepad(i);

            if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            {
                inventories[i].RemoveItem(selectedItem);
                pickups[i].ClearHandAfterTransfer();
                StartCoroutine(TransferObject(selectedItem));
            }
        }
    }

    Gamepad GetAssignedGamepad(int playerIndex)
    {
        if (Gamepad.all.Count <= playerIndex)
            return null;

        return Gamepad.all[playerIndex];
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        InventorySystem inventory = other.GetComponentInChildren<InventorySystem>(true);

        if (pickup == null || inventory == null) return;

        int index = pickup.playerIndex;
        if (index < 0 || index > 1) return;

        pickups[index] = pickup;
        inventories[index] = inventory;
        playersInside[index] = true;

        Debug.Log("Player " + (index + 1) + " entered tunnel");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        int index = pickup.playerIndex;
        if (index < 0 || index > 1) return;

        pickups[index] = null;
        inventories[index] = null;
        playersInside[index] = false;

        Debug.Log("Player " + (index + 1) + " exited tunnel");
    }

    void UpdateUI()
    {
        if (pressXUIPlayer1 != null)
        {
            bool showP1 =
                playersInside[0] &&
                inventories[0] != null &&
                inventories[0].GetSelectedItem() != null &&
                !isTransferring;

            pressXUIPlayer1.SetActive(showP1);
        }

        if (pressXUIPlayer2 != null)
        {
            bool showP2 =
                playersInside[1] &&
                inventories[1] != null &&
                inventories[1].GetSelectedItem() != null &&
                !isTransferring;

            pressXUIPlayer2.SetActive(showP2);
        }
    }

    void HideUI()
    {
        if (pressXUIPlayer1 != null) pressXUIPlayer1.SetActive(false);
        if (pressXUIPlayer2 != null) pressXUIPlayer2.SetActive(false);
    }

    IEnumerator TransferObject(GameObject objectToTransfer)
    {
        isTransferring = true;
        HideUI();

        objectToTransfer.SetActive(true);
        objectToTransfer.transform.SetParent(null);

        Rigidbody rb = objectToTransfer.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Transform startPoint = visualStartPoint != null ? visualStartPoint : inPoint;

        Vector3 startPos = startPoint.position;
        Quaternion startRot = startPoint.rotation;

        objectToTransfer.transform.position = startPos;
        objectToTransfer.transform.rotation = startRot;

        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            objectToTransfer.transform.position =
                Vector3.Lerp(startPos, inPoint.position, progress);

            objectToTransfer.transform.rotation =
                Quaternion.Lerp(startRot, inPoint.rotation, progress);

            yield return null;
        }

        objectToTransfer.transform.position = targetSpawnPoint.position;
        objectToTransfer.transform.rotation = targetSpawnPoint.rotation;
        objectToTransfer.tag = "Pickup";

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        isTransferring = false;
    }
}