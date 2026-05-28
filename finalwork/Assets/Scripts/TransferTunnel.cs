using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransferTunnel : MonoBehaviour
{
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
        int playerIndex = playerPickupSystem.playerIndex;

        bool keyboardPressed =
            playerIndex == 0 &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        Gamepad pad = GetAssignedGamepad(playerIndex);

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

        playerInventory = other.GetComponentInChildren<InventorySystem>(true);
        playerPickupSystem = other.GetComponentInChildren<PickupSystem>(true);

        if (playerInventory == null || playerPickupSystem == null) return;

        playerInside = true;
        usingController = true;

        SetPromptDisplayForPlayer();

        UpdateUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem exitingPickupSystem = other.GetComponentInChildren<PickupSystem>(true);

        if (exitingPickupSystem != playerPickupSystem) return;

        playerInside = false;
        playerInventory = null;
        playerPickupSystem = null;

        HideUI();
    }

    void UpdateUI()
    {
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
        
        Debug.Log("Tunnel UI check: " + showUI);
    }

    void HideUI()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
        if (pressXUI != null) pressXUI.SetActive(false);
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

    void SetPromptDisplayForPlayer()
{
    if (playerPickupSystem == null) return;

    int displayIndex = playerPickupSystem.playerIndex;

    Canvas pressECanvas = pressEUI != null ? pressEUI.GetComponentInParent<Canvas>(true) : null;
    Canvas pressXCanvas = pressXUI != null ? pressXUI.GetComponentInParent<Canvas>(true) : null;

    if (pressECanvas != null)
        pressECanvas.targetDisplay = displayIndex;

    if (pressXCanvas != null)
        pressXCanvas.targetDisplay = displayIndex;
}
}