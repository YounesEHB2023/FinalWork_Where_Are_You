using UnityEngine;
using UnityEngine.InputSystem;

public class PickupSystem : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform holdPoint;
    public Animator animator;
    public InventorySystem inventory;

    [Header("Pickup Settings")]
    public float pickupDistance = 3f;
    public float dropForwardForce = 1f;

    private GameObject heldObject;
    private Rigidbody heldRb;

    void Update()
    {
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
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SwitchToInventoryItem(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SwitchToInventoryItem(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SwitchToInventoryItem(2);
    }

    if (Gamepad.current != null)
    {
        if (Gamepad.current.dpad.left.wasPressedThisFrame || Gamepad.current.dpad.right.wasPressedThisFrame)
        {
            GameObject selectedItem = inventory.GetSelectedItem();

            if (selectedItem != null)
                HoldInventoryItem(selectedItem);
            else
                HideHeldObject();
        }
    }
}

void SwitchToInventoryItem(int slotIndex)
{
    if (inventory == null) return;

    inventory.SelectSlot(slotIndex);

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

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                GameObject obj = hit.collider.gameObject;

                ItemData itemData = obj.GetComponent<ItemData>();
                Sprite icon = itemData != null ? itemData.icon : null;

                bool added = inventory != null && inventory.AddItemToFirstEmptySlot(obj, icon);

                if (added)
                    PickObjectToHand(obj);
                else
                    Debug.Log("Inventory is full.");
            }
        }
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
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

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