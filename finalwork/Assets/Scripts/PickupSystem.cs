using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PickupSystem : NetworkBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform holdPoint;
    public Animator animator;
    public InventorySystem inventory;

    [Header("Pickup Settings")]
    public float pickupDistance = 4f;
    public float pickupRadius = 0.6f;
    public float dropForwardForce = 1f;
    public float dropDistanceFromPlayer = 1.5f;

    private GameObject heldObject;
    private Rigidbody heldRb;
    private GameObject heldVisual;

    private bool blockPickupInput = false;

    public GameObject GetHeldVisual()
    {
        return heldVisual;
    }

    public void SetPickupInputBlocked(bool blocked)
    {
        blockPickupInput = blocked;
    }

    public void ClearHandAfterTransfer()
    {
        if (heldVisual != null)
        {
            Destroy(heldVisual);
            heldVisual = null;
        }

        heldObject = null;
        heldRb = null;

        if (animator != null)
            animator.SetBool("IsHolding", false);

        UpdateHoldingStateServerRpc(false);
        RequestHideHeldVisualServerRpc();

        blockPickupInput = false;
    }

    void Update()
    {
        if (!IsOwner) return;
        if (!Application.isFocused) return;

        if (!blockPickupInput && PressedInteract())
        {
            if (heldObject == null)
                TryPickup();
            else
                DropHeldObject();
        }

        HandleInventorySelection();
    }

    void HandleInventorySelection()
    {
        if (inventory == null) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchToInventoryItem(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchToInventoryItem(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchToInventoryItem(2);
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.right.wasPressedThisFrame)
            {
                inventory.SelectNextSlot();
                SyncHandWithSelectedSlot();
            }

            if (Gamepad.current.dpad.left.wasPressedThisFrame)
            {
                inventory.SelectPreviousSlot();
                SyncHandWithSelectedSlot();
            }
        }
    }

    void SwitchToInventoryItem(int slotIndex)
    {
        inventory.SelectSlot(slotIndex);
        SyncHandWithSelectedSlot();
    }

    public void SyncHandWithSelectedSlot()
    {
        GameObject selectedItem = inventory.GetSelectedItem();

        if (selectedItem != null)
            HoldInventoryItem(selectedItem);
        else
            HideHeldObject();
    }

    bool PressedInteract()
    {
        bool keyboardPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool controllerPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        return keyboardPressed || controllerPressed;
    }

    void TryPickup()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.SphereCast(ray, pickupRadius, out RaycastHit hit, pickupDistance, ~0, QueryTriggerInteraction.Collide))
        {
            if (!hit.collider.CompareTag("Pickup")) return;

            GameObject obj = hit.collider.gameObject;

            NetworkObject netObj = obj.GetComponent<NetworkObject>();

            if (netObj == null)
            {
                Debug.LogWarning("Pickup object needs NetworkObject: " + obj.name);
                return;
            }

            ItemData itemData = obj.GetComponent<ItemData>();
            Sprite icon = itemData != null ? itemData.icon : null;

            bool added = inventory != null && inventory.AddItemToFirstEmptySlot(obj, icon);

            if (!added)
            {
                Debug.Log("Inventory is full.");
                return;
            }

            PickObjectToHand(obj);

            RequestShowHeldVisualServerRpc(netObj.NetworkObjectId);
            RequestHidePickupServerRpc(netObj.NetworkObjectId);
        }
    }

    void HoldInventoryItem(GameObject obj)
    {
        if (heldObject == obj && heldVisual != null)
            return;

        if (heldObject != null && heldObject != obj)
            HideHeldObject();

        PickObjectToHand(obj);

        NetworkObject netObj = obj.GetComponent<NetworkObject>();

        if (netObj != null)
            RequestShowHeldVisualServerRpc(netObj.NetworkObjectId);
    }

    void PickObjectToHand(GameObject obj)
    {
        heldObject = obj;
        heldRb = heldObject.GetComponent<Rigidbody>();

        heldObject.SetActive(false);

        if (animator != null)
            animator.SetBool("IsHolding", true);

        UpdateHoldingStateServerRpc(true);
    }

    void HideHeldObject()
    {
        if (heldVisual != null)
        {
            Destroy(heldVisual);
            heldVisual = null;
        }

        heldObject = null;
        heldRb = null;

        if (animator != null)
            animator.SetBool("IsHolding", false);

        UpdateHoldingStateServerRpc(false);
        RequestHideHeldVisualServerRpc();
    }

    void DropHeldObject()
    {
        if (heldObject == null) return;

        GameObject objectToDrop = heldObject;

        if (heldVisual != null)
        {
            Destroy(heldVisual);
            heldVisual = null;
        }

        NetworkObject netObj = objectToDrop.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogWarning("Dropped object needs NetworkObject: " + objectToDrop.name);
            return;
        }

        Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * dropDistanceFromPlayer;
        Quaternion dropRotation = Quaternion.identity;
        Vector3 forceDirection = playerCamera.transform.forward;

        if (inventory != null)
            inventory.RemoveItem(objectToDrop);

        heldObject = null;
        heldRb = null;

        if (animator != null)
            animator.SetBool("IsHolding", false);

        UpdateHoldingStateServerRpc(false);
        RequestHideHeldVisualServerRpc();

        RequestDropObjectServerRpc(netObj.NetworkObjectId, dropPosition, dropRotation, forceDirection);
    }

    [ServerRpc]
    void RequestHidePickupServerRpc(ulong networkObjectId)
    {
        HidePickupClientRpc(networkObjectId);
    }

    [ClientRpc]
    void HidePickupClientRpc(ulong networkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
            return;

        netObj.gameObject.SetActive(false);
    }

    [ServerRpc]
    void RequestDropObjectServerRpc(ulong networkObjectId, Vector3 position, Quaternion rotation, Vector3 forceDirection)
    {
        DropObjectClientRpc(networkObjectId, position, rotation, forceDirection);
    }

    [ClientRpc]
    void DropObjectClientRpc(ulong networkObjectId, Vector3 position, Quaternion rotation, Vector3 forceDirection)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
            return;

        GameObject obj = netObj.gameObject;

        obj.SetActive(true);
        obj.tag = "Pickup";
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(forceDirection * dropForwardForce, ForceMode.Impulse);
        }

        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            col.enabled = true;

        OutlineProximity outlineProximity = obj.GetComponent<OutlineProximity>();
        if (outlineProximity != null)
            outlineProximity.enabled = true;
    }

    [ServerRpc]
    void RequestShowHeldVisualServerRpc(ulong networkObjectId)
    {
        ShowHeldVisualClientRpc(networkObjectId);
    }

    [ClientRpc]
    void ShowHeldVisualClientRpc(ulong networkObjectId)
    {
        if (!IsOwner) return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
            return;

        if (heldVisual != null)
            Destroy(heldVisual);

        GameObject original = netObj.gameObject;
        heldVisual = Instantiate(original, holdPoint);

        heldVisual.SetActive(true);

        HeldItemSettings settings = original.GetComponent<HeldItemSettings>();

        if (settings != null)
        {
            heldVisual.transform.localPosition = settings.holdPosition;
            heldVisual.transform.localRotation = Quaternion.Euler(settings.holdRotation);
            heldVisual.transform.localScale = settings.holdScale;
        }
        else
        {
            heldVisual.transform.localPosition = Vector3.zero;
            heldVisual.transform.localRotation = Quaternion.identity;
        }

        NetworkObject visualNetObj = heldVisual.GetComponent<NetworkObject>();
        if (visualNetObj != null)
            Destroy(visualNetObj);

        Rigidbody visualRb = heldVisual.GetComponent<Rigidbody>();
        if (visualRb != null)
            Destroy(visualRb);

        Collider[] visualColliders = heldVisual.GetComponentsInChildren<Collider>();
        foreach (Collider col in visualColliders)
            Destroy(col);

        Outline outline = heldVisual.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        OutlineProximity outlineProximity = heldVisual.GetComponent<OutlineProximity>();
        if (outlineProximity != null)
            outlineProximity.enabled = false;
    }

    [ServerRpc]
    void RequestHideHeldVisualServerRpc()
    {
        HideHeldVisualClientRpc();
    }

    [ClientRpc]
    void HideHeldVisualClientRpc()
    {
        if (!IsOwner) return;

        if (heldVisual != null)
        {
            Destroy(heldVisual);
            heldVisual = null;
        }
    }

    [ServerRpc]
    void UpdateHoldingStateServerRpc(bool holding)
    {
        UpdateHoldingStateClientRpc(holding);
    }

    [ClientRpc]
    void UpdateHoldingStateClientRpc(bool holding)
    {
        if (animator != null)
            animator.SetBool("IsHolding", holding);
    }
}