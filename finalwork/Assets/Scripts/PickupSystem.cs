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

    public GameObject GetHeldVisual()
{
    return heldVisual;
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
}

   void Update()
{
    if (!IsOwner) return;
    if (!Application.isFocused) return;

    if (PressedInteract())
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

    void SyncHandWithSelectedSlot()
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

        GameObject obj = netObj.gameObject;
        obj.SetActive(false);
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
        RequestDropPickupServerRpc(netObj.NetworkObjectId, dropPosition, dropRotation, forceDirection);
    }

    [ServerRpc]
void RequestShowHeldVisualServerRpc(ulong networkObjectId)
{
    ShowHeldVisualClientRpc(networkObjectId);
}

[ClientRpc]
void ShowHeldVisualClientRpc(ulong networkObjectId)
{
    if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        return;

    GameObject originalObj = netObj.gameObject;

    if (heldVisual != null)
        Destroy(heldVisual);

   heldVisual = Instantiate(originalObj, holdPoint);

   Outline heldOutline = heldVisual.GetComponent<Outline>();
if (heldOutline != null)
    heldOutline.enabled = false;

OutlineProximity heldOutlineProximity = heldVisual.GetComponent<OutlineProximity>();
if (heldOutlineProximity != null)
    heldOutlineProximity.enabled = false;

Rigidbody visualRb = heldVisual.GetComponent<Rigidbody>();
if (visualRb != null)
    Destroy(visualRb);

    Collider[] colliders = heldVisual.GetComponentsInChildren<Collider>();
    foreach (Collider col in colliders)
        col.enabled = false;

    HeldItemSettings heldSettings = originalObj.GetComponent<HeldItemSettings>();

    if (heldSettings != null)
    {
        heldVisual.transform.localPosition = heldSettings.holdPosition;
        heldVisual.transform.localRotation = Quaternion.Euler(heldSettings.holdRotation);
        heldVisual.transform.localScale = heldSettings.holdScale;
    }
    else
    {
        heldVisual.transform.localPosition = Vector3.zero;
        heldVisual.transform.localRotation = Quaternion.identity;
        heldVisual.transform.localScale = Vector3.one;
    }

    heldVisual.SetActive(true);
}

[ServerRpc]
void RequestHideHeldVisualServerRpc()
{
    HideHeldVisualClientRpc();
}

[ClientRpc]
void HideHeldVisualClientRpc()
{
    if (heldVisual != null)
    {
        Destroy(heldVisual);
        heldVisual = null;
    }
}
[ServerRpc]
void RequestDropPickupServerRpc(ulong networkObjectId, Vector3 dropPosition, Quaternion dropRotation, Vector3 forceDirection)
{
    DropPickupClientRpc(networkObjectId, dropPosition, dropRotation, forceDirection);
}

[ClientRpc]
void DropPickupClientRpc(ulong networkObjectId, Vector3 dropPosition, Quaternion dropRotation, Vector3 forceDirection)
{
    if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        return;

    GameObject obj = netObj.gameObject;

    obj.transform.SetParent(null);
    obj.transform.position = dropPosition;
    obj.transform.rotation = dropRotation;
    obj.SetActive(true);

    Rigidbody rb = obj.GetComponent<Rigidbody>();

    if (rb != null)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(forceDirection * dropForwardForce, ForceMode.Impulse);
    }
}
[ServerRpc]
void UpdateHoldingStateServerRpc(bool isHolding)
{
    UpdateHoldingStateClientRpc(isHolding);
}

[ClientRpc]
void UpdateHoldingStateClientRpc(bool isHolding)
{
    if (animator != null)
        animator.SetBool("IsHolding", isHolding);
}
}