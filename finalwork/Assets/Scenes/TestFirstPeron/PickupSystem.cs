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

        HandleInventoryInput();
    }

    void HandleInventoryInput()
    {
        // Keyboard: 1, 2, 3
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                HandleKeyboardSlot(0);

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                HandleKeyboardSlot(1);

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
                HandleKeyboardSlot(2);
        }

        // Controller: D-pad down
        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.down.wasPressedThisFrame)
            {
                if (heldObject != null)
                    StoreHeldObjectInSelectedSlot();
                else
                    TakeSelectedItemFromInventory();
            }
        }
    }

    void HandleKeyboardSlot(int slotIndex)
{
    if (inventory == null) return;

    inventory.SelectSlot(slotIndex);

    if (heldObject != null)
    {
        // 👉 si tu tiens un objet → stocker
        StoreHeldObjectInSlot(slotIndex);
    }
    else
    {
        // 👉 sinon → récupérer depuis inventaire
        TakeSelectedItemFromInventory();
    }
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
                PickObjectToHand(hit.collider.gameObject);
        }
    }

    void PickObjectToHand(GameObject obj)
    {
        heldObject = obj;
        heldRb = heldObject.GetComponent<Rigidbody>();

        if (heldRb != null)
        {
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.useGravity = false;
            heldRb.isKinematic = true;
        }

        heldObject.SetActive(true);
        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        if (animator != null)
            animator.SetBool("IsHolding", true);
    }

    void StoreHeldObjectInSelectedSlot()
    {
        if (inventory == null) return;

        StoreHeldObjectInSlot(-1);
    }

    void StoreHeldObjectInSlot(int slotIndex)
    {
        if (inventory == null || heldObject == null) return;

        ItemData itemData = heldObject.GetComponent<ItemData>();
        Sprite icon = itemData != null ? itemData.icon : null;

        bool added;

        if (slotIndex >= 0)
            added = inventory.AddItemToSlot(slotIndex, heldObject, icon);
        else
            added = inventory.AddItemToSelectedSlot(heldObject, icon);

        if (added)
        {
            heldObject.transform.SetParent(null);
            heldObject.SetActive(false);

            heldObject = null;
            heldRb = null;

            if (animator != null)
                animator.SetBool("IsHolding", false);
        }
        else
        {
            Debug.Log("Slot is full.");
        }
    }

    void TakeSelectedItemFromInventory()
    {
        if (inventory == null) return;

        GameObject item = inventory.GetSelectedItem();

        if (item != null)
        {
            inventory.RemoveSelectedItem();
            PickObjectToHand(item);
        }
    }

    void DropHeldObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = true;

            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;

            heldRb.AddForce(playerCamera.transform.forward * dropForwardForce, ForceMode.Impulse);
        }

        if (animator != null)
            animator.SetBool("IsHolding", false);

        heldObject = null;
        heldRb = null;
    }
}