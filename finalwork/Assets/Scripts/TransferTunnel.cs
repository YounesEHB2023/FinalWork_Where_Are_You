using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransferTunnel : MonoBehaviour
{
    [Header("Owner")]
    public int ownerPlayerIndex = 0; // Player 1 = 0, Player 2 = 1

    [Header("Points")]
    public Transform visualStartPoint;
    public Transform inPoint;
    public Transform targetSpawnPoint;

    [Header("UI")]
    public GameObject pressEUI;
    public GameObject pressXUI;

    private bool playerInside;
    private bool isTransferring;
    private bool usingController = true;

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
        UpdateUI();

        if (!playerInside) return;
        if (isTransferring) return;
        if (playerInventory == null) return;
        if (playerPickupSystem == null) return;

        GameObject selectedItem = playerInventory.GetSelectedItem();
        if (selectedItem == null) return;

        if (PressedTransfer())
        {
            playerInventory.RemoveItem(selectedItem);
            playerPickupSystem.ClearHandAfterTransfer();

            StartCoroutine(TransferObject(selectedItem));
        }
    }

    bool PressedTransfer()
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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        InventorySystem inventory = other.GetComponentInChildren<InventorySystem>(true);

        if (pickup == null || inventory == null) return;

        if (pickup.playerIndex != ownerPlayerIndex) return;

        playerPickupSystem = pickup;
        playerInventory = inventory;
        playerInside = true;

        Debug.Log("Tunnel owner " + ownerPlayerIndex + " entered by player " + pickup.playerIndex);

        UpdateUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup != playerPickupSystem) return;

        playerInside = false;
        playerPickupSystem = null;
        playerInventory = null;

        HideUI();
    }

    void UpdateUI()
    {
        bool showUI =
            playerInside &&
            playerInventory != null &&
            playerInventory.GetSelectedItem() != null &&
            !isTransferring;

        if (!showUI)
        {
            HideUI();
            return;
        }

        if (pressEUI != null)
            pressEUI.SetActive(!usingController);

        if (pressXUI != null)
            pressXUI.SetActive(usingController);
    }

    void HideUI()
    {
        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (pressXUI != null)
            pressXUI.SetActive(false);
    }

    void DetectInputDevice()
    {
        Gamepad pad = GetAssignedGamepad(ownerPlayerIndex);

        if (pad != null)
        {
            Vector2 dpad = pad.dpad.ReadValue();
            Vector2 stick = pad.leftStick.ReadValue();
            Vector2 rightStick = pad.rightStick.ReadValue();

            if (dpad != Vector2.zero || stick.magnitude > 0.1f || rightStick.magnitude > 0.1f)
                usingController = true;
        }

        if (ownerPlayerIndex == 0 && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (ownerPlayerIndex == 0 && Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
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

            Quaternion smoothRotation =
                Quaternion.Lerp(startRot, inPoint.rotation, progress);

            Quaternion spinRotation =
                Quaternion.Euler(0f, progress * 180f, 0f);

            objectToTransfer.transform.rotation =
                smoothRotation * spinRotation;

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
        UpdateUI();
    }
}