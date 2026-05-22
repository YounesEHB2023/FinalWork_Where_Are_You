using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class WeaponPedestalSocket : MonoBehaviour
{
    [Header("Placement")]
    public Transform placePoint;
    public Vector3 placedRotationOffset;
    public float placeAnimationDuration = 0.35f;

    [Header("Floating")]
    public float floatHeight = 0.08f;
    public float floatSpeed = 2f;

    [Header("UI")]
    public GameObject pressEUI;
    public GameObject pressXUI;

    private static WeaponPedestalSocket activeSocket;

    private bool playerInside;
    private bool usingController;
    private bool isPlacing;

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

        if (playerInside)
            UpdateActiveSocket();

        bool isActive = activeSocket == this;

        if (!isActive)
{
    return;
}

        UpdateUI();

        if (isPlacing) return;
        if (playerInventory == null) return;

        bool pressed =
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (!pressed) return;

        GameObject selectedItem = playerInventory.GetSelectedItem();

        if (placedWeapon != null && selectedItem == null)
        {
            TakeBackPlacedWeapon();
            return;
        }

        if (selectedItem == null) return;

        if (placedWeapon != null) return;

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

    void UpdateActiveSocket()
    {
        if (currentPlayer == null) return;

        if (activeSocket == null)
        {
            activeSocket = this;
            return;
        }

        if (!activeSocket.playerInside)
        {
            activeSocket = this;
            return;
        }

        float myDistance = Vector3.Distance(currentPlayer.position, transform.position);
        float activeDistance = Vector3.Distance(currentPlayer.position, activeSocket.transform.position);

        if (myDistance < activeDistance)
            activeSocket = this;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj != null && !playerNetObj.IsOwner) return;

        currentPlayer = other.transform;
        playerInventory = other.GetComponentInChildren<InventorySystem>(true);
        playerPickupSystem = other.GetComponentInChildren<PickupSystem>(true);

        playerInside = true;
        activeSocket = this;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj != null && !playerNetObj.IsOwner) return;

        playerInside = false;

        if (activeSocket == this)
            activeSocket = null;

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
            (
                hasSelectedWeapon ||
                placedWeapon != null
            );

        if (pressEUI != null)
            pressEUI.SetActive(canShow && !usingController);

        if (pressXUI != null)
            pressXUI.SetActive(canShow && usingController);
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

    Vector3 positionOffset = Vector3.zero;
    Vector3 rotationOffset = Vector3.zero;
    Vector3 finalScale = weapon.transform.localScale;

    if (settings != null)
    {
        positionOffset = settings.pedestalPositionOffset;
        rotationOffset = settings.pedestalRotationOffset;
        finalScale = settings.pedestalScale;
    }

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
    playerPickupSystem.SyncHandWithSelectedSlot();

        UpdateUI();
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
                Gamepad.current.buttonSouth.wasPressedThisFrame
            )
                usingController = true;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
    }
}