using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class TransferTunnel : NetworkBehaviour
{
    public Transform inPoint;
    public Transform targetSpawnPoint;

    public GameObject pressEUI;
    public GameObject pressXUI;

    private bool playerInside;
    private bool isTransferring;
    private bool usingController;

    private static TransferTunnel activeTunnel;

    private InventorySystem playerInventory;
    private PickupSystem playerPickupSystem;

    void Start()
    {
        HideUI();
    }

    void Update()
    {
        if (!Application.isFocused) return;

        DetectInputDevice();

        if (activeTunnel != this) return;

        UpdateUI();

        bool keyboardTransfer =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        bool controllerTransfer =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (
            playerInside &&
            playerInventory != null &&
            playerInventory.GetSelectedItem() != null &&
            !isTransferring &&
            (keyboardTransfer || controllerTransfer)
        )
        {
            GameObject selectedItem = playerInventory.GetSelectedItem();
            NetworkObject netObj = selectedItem.GetComponent<NetworkObject>();

            if (netObj == null)
            {
                Debug.LogWarning("Transfer object needs NetworkObject: " + selectedItem.name);
                return;
            }

            RequestTransferServerRpc(netObj.NetworkObjectId);

            playerInventory.RemoveItem(selectedItem);

            if (playerPickupSystem != null)
                playerPickupSystem.ClearHandAfterTransfer();
        }
    }

    void DetectInputDevice()
    {
        if (Gamepad.current != null)
        {
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();

            if (
                dpad != Vector2.zero ||
                leftStick.magnitude > 0.1f ||
                rightStick.magnitude > 0.1f ||
                Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame
            )
            {
                usingController = true;
            }
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj != null && !playerNetObj.IsOwner) return;

        playerInventory = other.GetComponentInChildren<InventorySystem>(true);
        playerPickupSystem = other.GetComponentInChildren<PickupSystem>(true);

        playerInside = true;
        activeTunnel = this;

        UpdateUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj != null && !playerNetObj.IsOwner) return;

        playerInside = false;

        if (activeTunnel == this)
        {
            activeTunnel = null;
            playerInventory = null;
            playerPickupSystem = null;
            HideUI();
        }
    }

    void UpdateUI()
    {
        if (activeTunnel != this)
            return;

        bool hasSelectedItem =
            playerInventory != null &&
            playerInventory.GetSelectedItem() != null;

        bool showUI =
            playerInside &&
            hasSelectedItem &&
            !isTransferring;

        if (pressEUI != null)
            pressEUI.SetActive(showUI && !usingController);

        if (pressXUI != null)
            pressXUI.SetActive(showUI && usingController);
    }

    void HideUI()
    {
        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (pressXUI != null)
            pressXUI.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestTransferServerRpc(ulong networkObjectId)
    {
        TransferObjectClientRpc(networkObjectId);
    }

    [ClientRpc]
    void TransferObjectClientRpc(ulong networkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
            return;

        StartCoroutine(TransferObject(netObj.gameObject));
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

        Vector3 startPos = inPoint.position;
        Quaternion startRot = inPoint.rotation;

        Vector3 endPos = targetSpawnPoint.position;
        Quaternion endRot = targetSpawnPoint.rotation;

        objectToTransfer.transform.position = startPos;
        objectToTransfer.transform.rotation = startRot;

        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / duration);

            objectToTransfer.transform.position =
                Vector3.Lerp(startPos, endPos, progress);

            objectToTransfer.transform.rotation =
                Quaternion.Lerp(startRot, endRot, progress);

            yield return null;
        }

        objectToTransfer.transform.position = endPos;
        objectToTransfer.transform.rotation = endRot;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        isTransferring = false;
        UpdateUI();
    }
}