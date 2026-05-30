using UnityEngine;
using UnityEngine.InputSystem;

public class PickupSystem : MonoBehaviour
{
    [Header("Local Player")]
    public int playerIndex = 0;

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

    [Header("Crosshair")]
public RectTransform crosshair;
public float normalCrosshairScale = 1f;
public float pickupCrosshairScale = 1.6f;
public float crosshairSmoothSpeed = 12f;

private GameObject currentPickupTarget;

    private GameObject heldObject;
    private Rigidbody heldRb;
    private GameObject heldVisual;

    private bool blockPickupInput = false;

    void Awake()
    {
        FirstPersonController controller = GetComponentInParent<FirstPersonController>();
        if (controller != null)
            playerIndex = controller.playerIndex;

        if (inventory != null)
            inventory.playerIndex = playerIndex;
    }

    void Update()
    {
        if (!Application.isFocused) return;

        UpdatePickupTarget();

        if (!blockPickupInput && PressedInteract())
        {
            if (heldObject == null)
                TryPickup();
            else
                DropHeldObject();
        }

        HandleInventorySelection();
    }

    Gamepad GetAssignedGamepad()
    {
        if (Gamepad.all.Count <= playerIndex)
            return null;

        return Gamepad.all[playerIndex];
    }

    bool PressedInteract()
    {
        Gamepad pad = GetAssignedGamepad();

        bool keyboardPressed =
            playerIndex == 0 &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        bool controllerPressed =
            pad != null &&
            pad.buttonSouth.wasPressedThisFrame;

        return keyboardPressed || controllerPressed;
    }

    void HandleInventorySelection()
    {
        if (inventory == null) return;

        if (playerIndex == 0 && Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchToInventoryItem(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchToInventoryItem(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchToInventoryItem(2);
        }

        Gamepad pad = GetAssignedGamepad();

        if (pad != null)
        {
            if (pad.dpad.right.wasPressedThisFrame)
            {
                inventory.SelectNextSlot();
                SyncHandWithSelectedSlot();
            }

            if (pad.dpad.left.wasPressedThisFrame)
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

    public GameObject GetHeldVisual()
    {
        return heldVisual;
    }

    public void SetPickupInputBlocked(bool blocked)
    {
        blockPickupInput = blocked;
    }

    public void SyncHandWithSelectedSlot()
    {
        if (inventory == null) return;

        GameObject selectedItem = inventory.GetSelectedItem();

        if (selectedItem != null)
            HoldInventoryItem(selectedItem);
        else
            HideHeldObject();
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
    }

    void TryPickup()
{
    if (currentPickupTarget == null) return;

    GameObject obj = currentPickupTarget;

    ItemData itemData = obj.GetComponent<ItemData>();
    Sprite icon = itemData != null ? itemData.icon : null;

    bool added = inventory != null && inventory.AddItemToFirstEmptySlot(obj, icon);

    if (!added)
    {
        Debug.Log("Inventory is full.");
        return;
    }

    PickObjectToHand(obj);
}

    void HoldInventoryItem(GameObject obj)
    {
        if (heldObject == obj && heldVisual != null)
            return;

        if (heldObject != null && heldObject != obj)
            HideHeldObject();

        PickObjectToHand(obj);
    }

    void PickObjectToHand(GameObject obj)
    {
        heldObject = obj;
        heldRb = heldObject.GetComponent<Rigidbody>();

        CreateHeldVisual(obj);
heldObject.SetActive(false);

        if (animator != null)
            animator.SetBool("IsHolding", true);
    }

    void CreateHeldVisual(GameObject original)
    {
        if (heldVisual != null)
            Destroy(heldVisual);

        heldVisual = Instantiate(original, holdPoint);
        heldVisual.SetActive(true);
        heldVisual.name = original.name + "_HeldVisual";

        heldVisual.transform.localPosition = Vector3.zero;
        heldVisual.transform.localRotation = Quaternion.identity;
        heldVisual.transform.localScale = Vector3.one;

        HeldItemSettings settings = original.GetComponent<HeldItemSettings>();

        if (settings != null)
        {
            heldVisual.transform.localPosition = settings.holdPosition;
            heldVisual.transform.localRotation = Quaternion.Euler(settings.holdRotation);
            heldVisual.transform.localScale = settings.holdScale;
        }

        Rigidbody rb = heldVisual.GetComponent<Rigidbody>();
        if (rb != null)
            Destroy(rb);

        Collider[] colliders = heldVisual.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            col.enabled = false;

        Outline outline = heldVisual.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        OutlineProximity outlineProximity = heldVisual.GetComponent<OutlineProximity>();
        if (outlineProximity != null)
            outlineProximity.enabled = false;
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

        if (inventory != null)
            inventory.RemoveItem(objectToDrop);

        heldObject = null;
        heldRb = null;

        objectToDrop.SetActive(true);

        objectToDrop.transform.position =
            playerCamera.transform.position + playerCamera.transform.forward * dropDistanceFromPlayer;

        objectToDrop.transform.rotation = Quaternion.identity;

        Rigidbody rb = objectToDrop.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(playerCamera.transform.forward * dropForwardForce, ForceMode.Impulse);
        }

        if (animator != null)
            animator.SetBool("IsHolding", false);
    }

    void UpdatePickupTarget()
{
    currentPickupTarget = FindPickupTarget();

    float targetScale = currentPickupTarget != null
        ? pickupCrosshairScale
        : normalCrosshairScale;

    if (crosshair != null)
    {
        Vector3 target = Vector3.one * targetScale;

        crosshair.localScale = Vector3.Lerp(
            crosshair.localScale,
            target,
            Time.deltaTime * crosshairSmoothSpeed
        );
    }
}

GameObject FindPickupTarget()
{
    if (playerCamera == null) return null;

    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

    if (Physics.SphereCast(ray, pickupRadius, out RaycastHit hit, pickupDistance, ~0, QueryTriggerInteraction.Collide))
    {
        GameObject pickup = GetPickupObject(hit.collider);

        if (pickup != null)
            return pickup;
    }

    return null;
}

GameObject GetPickupObject(Collider col)
{
    if (col == null) return null;

    if (col.CompareTag("Pickup"))
        return col.gameObject;

    Transform current = col.transform;

    while (current != null)
    {
        if (current.CompareTag("Pickup"))
            return current.gameObject;

        if (current.GetComponent<ItemData>() != null)
            return current.gameObject;

        current = current.parent;
    }

    return null;
}
}