using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPedestalSocket : MonoBehaviour
{
    [Header("Owner")]
    public int ownerPlayerIndex = 0;

    [Header("Puzzle")]
    public OudheidWeaponPuzzleManager puzzleManager;
    public string expectedWeaponName;

    [Header("Placement")]
    public Transform placePoint;
    public float placeAnimationDuration = 0.35f;

    [Header("Floating")]
    public float floatHeight = 0.08f;
    public float floatSpeed = 2f;

    [Header("UI")]
    public GameObject pressEUI;
    public GameObject pressXUI;


    private bool playerInside;
    private bool usingController = true;
    private bool isPlacing;
    private bool isLocked;

    private Transform currentPlayer;
    private InventorySystem playerInventory;
    private PickupSystem playerPickupSystem;

    private GameObject placedWeapon;
    private Coroutine floatingRoutine;

    void Start()
    {
        HideUI();
    }

    void Update()
    {
        if (!Application.isFocused) return;

        DetectInputDevice();

        

        UpdateUI();

        if (isPlacing || isLocked) return;
        if (playerInventory == null) return;

        if (!PressedInteract()) return;

        if (placedWeapon != null)
        {
            TakeBackPlacedWeapon();
            return;
        }

        GameObject selectedItem = playerInventory.GetSelectedItem();
        if (selectedItem == null) return;

        Vector3 startPos = selectedItem.transform.position;
        Quaternion startRot = selectedItem.transform.rotation;

        if (playerPickupSystem != null && playerPickupSystem.GetHeldVisual() != null)
        {
            startPos = playerPickupSystem.GetHeldVisual().transform.position;
            startRot = playerPickupSystem.GetHeldVisual().transform.rotation;
        }

        playerInventory.RemoveItem(selectedItem);

        if (playerPickupSystem != null)
            playerPickupSystem.ClearHandAfterTransfer();

        StartCoroutine(PlaceWeaponAnimation(selectedItem, startPos, startRot));
    }

    bool PressedInteract()
    {
        bool keyboardPressed =
            ownerPlayerIndex == 0 &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        Gamepad pad = GetAssignedGamepad(ownerPlayerIndex);

        bool controllerPressed =
            pad != null &&
            pad.buttonSouth.wasPressedThisFrame;

        return keyboardPressed || controllerPressed;
    }

    Gamepad GetAssignedGamepad(int playerIndex)
    {
        if (Gamepad.all.Count <= playerIndex)
            return null;

        return Gamepad.all[playerIndex];
    }

    public bool HasWeaponPlaced()
    {
        return placedWeapon != null;
    }

    public bool IsCorrectWeaponPlaced()
    {
        if (placedWeapon == null) return false;
        return placedWeapon.name.ToLower().Contains(expectedWeaponName.ToLower());
    }

    public void LockPlacedWeapon()
    {
        isLocked = true;
        HideUI();
    }

    

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        InventorySystem inventory = other.GetComponentInChildren<InventorySystem>(true);

        if (pickup == null || inventory == null) return;
        if (pickup.playerIndex != ownerPlayerIndex) return;

        currentPlayer = other.transform;
        playerInventory = inventory;
        playerPickupSystem = pickup;

        playerInside = true;
        Debug.Log("ENTER SOCKET: " + gameObject.name + " | owner: " + ownerPlayerIndex);

        playerPickupSystem.SetPickupInputBlocked(true);

        UpdateUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;
        if (pickup != playerPickupSystem) return;

        playerInside = false;

        

        if (playerPickupSystem != null)
            playerPickupSystem.SetPickupInputBlocked(false);

        currentPlayer = null;
        playerInventory = null;
        playerPickupSystem = null;

        HideUI();
    }

    void UpdateUI()
    {
        bool hasSelectedWeapon =
            playerInventory != null &&
            playerInventory.GetSelectedItem() != null;

        bool canShow =
            playerInside &&
            !isPlacing &&
            !isLocked &&
            (hasSelectedWeapon || placedWeapon != null);

        if (!canShow)
    return;

Debug.Log("SHOW UI FROM: " + gameObject.name);
        if (pressEUI != null)
            pressEUI.SetActive(!usingController);

        if (pressXUI != null)
            pressXUI.SetActive(usingController);
    }

    void HideUI()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
        if (pressXUI != null) pressXUI.SetActive(false);
    }

    IEnumerator PlaceWeaponAnimation(GameObject weapon, Vector3 startPos, Quaternion startRot)
    {
        isPlacing = true;
        placedWeapon = weapon;
        HideUI();

        weapon.SetActive(true);
        weapon.transform.SetParent(null);
        weapon.tag = "Untagged";

        Rigidbody rb = weapon.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] colliders = weapon.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            col.enabled = false;

        Outline outline = weapon.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        OutlineProximity outlineProximity = weapon.GetComponent<OutlineProximity>();
        if (outlineProximity != null)
            outlineProximity.enabled = false;

        HeldItemSettings settings = weapon.GetComponent<HeldItemSettings>();

        Vector3 positionOffset = settings != null ? settings.pedestalPositionOffset : Vector3.zero;
        Vector3 rotationOffset = settings != null ? settings.pedestalRotationOffset : Vector3.zero;
        Vector3 finalScale = settings != null ? settings.pedestalScale : weapon.transform.localScale;

        Vector3 endPos = placePoint.position + placePoint.TransformDirection(positionOffset);
        Quaternion endRot = placePoint.rotation * Quaternion.Euler(rotationOffset);

        float t = 0f;

        while (t < placeAnimationDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / placeAnimationDuration);

            weapon.transform.position = Vector3.Lerp(startPos, endPos, p);
            weapon.transform.rotation = Quaternion.Lerp(startRot, endRot, p);
            weapon.transform.localScale = Vector3.Lerp(weapon.transform.localScale, finalScale, p);

            yield return null;
        }

        weapon.transform.position = endPos;
        weapon.transform.rotation = endRot;
        weapon.transform.localScale = finalScale;

        isPlacing = false;
        floatingRoutine = StartCoroutine(FloatingLoop(weapon));

        if (puzzleManager != null)
            puzzleManager.CheckPuzzle();
    }

    IEnumerator FloatingLoop(GameObject weapon)
    {
        while (weapon != null)
        {
            HeldItemSettings settings = weapon.GetComponent<HeldItemSettings>();

            Vector3 positionOffset = settings != null ? settings.pedestalPositionOffset : Vector3.zero;
            Vector3 rotationOffset = settings != null ? settings.pedestalRotationOffset : Vector3.zero;

            Vector3 finalPosition =
                placePoint.position + placePoint.TransformDirection(positionOffset);

            Quaternion finalRotation =
                placePoint.rotation * Quaternion.Euler(rotationOffset);

            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

            weapon.transform.position = finalPosition + Vector3.up * yOffset;
            weapon.transform.rotation = finalRotation;

            yield return null;
        }
    }

    void TakeBackPlacedWeapon()
    {
        if (placedWeapon == null) return;
        if (playerInventory == null) return;
        if (isLocked) return;

        ItemData itemData = placedWeapon.GetComponent<ItemData>();
        Sprite icon = itemData != null ? itemData.icon : null;

        bool added = playerInventory.AddItemToFirstEmptySlot(placedWeapon, icon);

        if (!added)
        {
            Debug.Log("Inventory full.");
            return;
        }

        if (floatingRoutine != null)
            StopCoroutine(floatingRoutine);

        placedWeapon.tag = "Pickup";

        Collider[] colliders = placedWeapon.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            col.enabled = true;

        placedWeapon.SetActive(false);
        placedWeapon = null;

        if (playerPickupSystem != null)
        {
            playerPickupSystem.SetPickupInputBlocked(true);
            playerPickupSystem.SyncHandWithSelectedSlot();
        }

        UpdateUI();

        if (puzzleManager != null)
            puzzleManager.CheckPuzzle();
    }

    void DetectInputDevice()
    {
        Gamepad pad = GetAssignedGamepad(ownerPlayerIndex);

        if (pad != null)
        {
            Vector2 dpad = pad.dpad.ReadValue();
            Vector2 leftStick = pad.leftStick.ReadValue();
            Vector2 rightStick = pad.rightStick.ReadValue();

            if (
                dpad != Vector2.zero ||
                leftStick.magnitude > 0.1f ||
                rightStick.magnitude > 0.1f ||
                pad.buttonSouth.wasPressedThisFrame
            )
            {
                usingController = true;
            }
        }

        if (ownerPlayerIndex == 0 && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (ownerPlayerIndex == 0 && Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
    }
}