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

    private GameObject heldObject;
    private Rigidbody heldRb;

    void Update()
    {
        if (!IsOwner) return;

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

        if (Physics.SphereCast(ray, pickupRadius, out RaycastHit hit, pickupDistance))
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
        if (heldObject == obj) return;

        if (heldObject != null)
            HideHeldObject();

        PickObjectToHand(obj);
    }

    void PickObjectToHand(GameObject obj)
    {
        heldObject = obj;
        heldRb = heldObject.GetComponent<Rigidbody>();

        heldObject.SetActive(true);

        if (heldRb != null)
        {
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.useGravity = false;
            heldRb.isKinematic = true;
        }

        heldObject.transform.SetParent(holdPoint);

        HeldItemSettings heldSettings = heldObject.GetComponent<HeldItemSettings>();

        if (heldSettings != null)
        {
            heldObject.transform.localPosition = heldSettings.holdPosition;
            heldObject.transform.localRotation = Quaternion.Euler(heldSettings.holdRotation);
            heldObject.transform.localScale = heldSettings.holdScale;
        }
        else
        {
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;
            heldObject.transform.localScale = Vector3.one;
        }

        if (animator != null)
            animator.SetBool("IsHolding", true);
    }

    void HideHeldObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);
        heldObject.SetActive(false);

        heldObject = null;
        heldRb = null;

        if (animator != null)
            animator.SetBool("IsHolding", false);
    }

    void DropHeldObject()
    {
        if (heldObject == null) return;

        GameObject objectToDrop = heldObject;

        if (inventory != null)
            inventory.RemoveItem(objectToDrop);

        objectToDrop.transform.SetParent(null);
        objectToDrop.SetActive(true);

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = true;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.AddForce(playerCamera.transform.forward * dropForwardForce, ForceMode.Impulse);
        }

        heldObject = null;
        heldRb = null;

        if (animator != null)
            animator.SetBool("IsHolding", false);
    }
}