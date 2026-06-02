using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorkbenchCraft : MonoBehaviour
{
    [Header("Owner")]
    public int ownerPlayerIndex = 1; // Player 2 = 1

    [Header("Slots")]
    public Transform[] slots;
    public Transform craftCenter;
    public Transform axeSpawnPoint;
    public Transform visualStartPoint;

    [Header("Craft Result")]
    public GameObject axePrefab;
    public Vector3 axeFinalScale = Vector3.one;

    [Header("UI")]
    public GameObject pressEUI;
    public GameObject pressXUI;

    [Header("Animation")]
    public float placeDuration = 0.8f;
    public float fusionDuration = 0.6f;
    public float axeAppearDuration = 0.5f;
    public float wrongThrowForce = 4f;
    public float wrongUpForce = 2f;

    private List<GameObject> placedObjects = new List<GameObject>();

    private bool crafted = false;
    private bool playerInside = false;
    private bool usingController = true;
    private bool isPlacing = false;
    private bool isCrafting = false;

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
        if (playerInventory == null) return;
        if (playerPickupSystem == null) return;
        if (crafted || isPlacing || isCrafting) return;
        if (placedObjects.Count >= slots.Length) return;

        GameObject selectedItem = playerInventory.GetSelectedItem();
        if (selectedItem == null) return;

        if (PressedPlace())
        {
            Vector3 startPos = selectedItem.transform.position;
            Quaternion startRot = selectedItem.transform.rotation;

            if (playerPickupSystem.GetHeldVisual() != null)
            {
                startPos = playerPickupSystem.GetHeldVisual().transform.position;
                startRot = playerPickupSystem.GetHeldVisual().transform.rotation;
            }

            playerInventory.RemoveItem(selectedItem);
            playerPickupSystem.ClearHandAfterTransfer();

            StartCoroutine(PlaceObjectAnimated(selectedItem, startPos, startRot));
        }
    }

    bool PressedPlace()
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

    void DetectInputDevice()
    {
        Gamepad pad = GetAssignedGamepad(ownerPlayerIndex);

        if (pad != null)
        {
            Vector2 dpad = pad.dpad.ReadValue();
            Vector2 leftStick = pad.leftStick.ReadValue();
            Vector2 rightStick = pad.rightStick.ReadValue();

            if (dpad != Vector2.zero || leftStick.magnitude > 0.1f || rightStick.magnitude > 0.1f)
                usingController = true;
        }

        if (ownerPlayerIndex == 0 && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (ownerPlayerIndex == 0 && Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
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

        UpdateUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;
        if (pickup != playerPickupSystem) return;

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
            !crafted &&
            !isPlacing &&
            !isCrafting &&
            placedObjects.Count < slots.Length;

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
        if (pressEUI != null) pressEUI.SetActive(false);
        if (pressXUI != null) pressXUI.SetActive(false);
    }

    IEnumerator PlaceObjectAnimated(GameObject obj, Vector3 startPos, Quaternion startRot)
    {
        isPlacing = true;
        HideUI();

        placedObjects.Add(obj);

        int slotIndex = placedObjects.Count - 1;
        Transform targetSlot = slots[slotIndex];

        obj.SetActive(true);
        obj.transform.SetParent(null);
        obj.tag = "Untagged";

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        obj.transform.position = startPos;
        obj.transform.rotation = startRot;

        float t = 0f;

        while (t < placeDuration)
        {
            t += Time.deltaTime;

            float progress = t / placeDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            Vector3 arcOffset = Vector3.up * Mathf.Sin(smoothProgress * Mathf.PI) * 0.25f;

            obj.transform.position =
                Vector3.Lerp(startPos, targetSlot.position, smoothProgress) + arcOffset;

            obj.transform.rotation =
                Quaternion.Lerp(startRot, targetSlot.rotation, smoothProgress);

            yield return null;
        }

        obj.transform.position = targetSlot.position;
        obj.transform.rotation = targetSlot.rotation;

        isPlacing = false;

        CheckRecipe();
        UpdateUI();
    }

    void CheckRecipe()
    {
        if (placedObjects.Count < 3) return;

        bool hasStone = false;
        bool hasStick = false;
        bool hasVine = false;

        foreach (GameObject obj in placedObjects)
        {
            if (obj.name.Contains("Correct_Stone")) hasStone = true;
            if (obj.name.Contains("Correct_Stick")) hasStick = true;
            if (obj.name.Contains("Correct_Vine")) hasVine = true;
        }

        if (hasStone && hasStick && hasVine)
            StartCoroutine(CorrectCraftAnimation());
        else
            StartCoroutine(WrongCraftAnimation());
    }

    IEnumerator CorrectCraftAnimation()
    {
        isCrafting = true;
        HideUI();

        Vector3[] startPositions = new Vector3[placedObjects.Count];
        Vector3[] startScales = new Vector3[placedObjects.Count];

        for (int i = 0; i < placedObjects.Count; i++)
        {
            startPositions[i] = placedObjects[i].transform.position;
            startScales[i] = placedObjects[i].transform.localScale;
        }

        float t = 0f;

        while (t < fusionDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / fusionDuration);

            for (int i = 0; i < placedObjects.Count; i++)
            {
                Vector3 shake = Random.insideUnitSphere * 0.03f;
                shake.y = 0f;

                placedObjects[i].transform.position =
                    Vector3.Lerp(startPositions[i], craftCenter.position, progress) + shake;

                placedObjects[i].transform.localScale =
                    Vector3.Lerp(startScales[i], Vector3.zero, progress);
            }

            yield return null;
        }

        foreach (GameObject obj in placedObjects)
        {
            Destroy(obj);
        }

        placedObjects.Clear();

        GameObject axe = Instantiate(axePrefab, axeSpawnPoint.position, axeSpawnPoint.rotation);
        axe.SetActive(true);
        axe.tag = "Pickup";

        StartCoroutine(ShowAxeAnimation(axe));

        crafted = true;
        isCrafting = false;

        Debug.Log("AXE CREATED");
    }

    IEnumerator ShowAxeAnimation(GameObject axe)
    {
        axe.transform.position = axeSpawnPoint.position;
        axe.transform.rotation = axeSpawnPoint.rotation;

        Vector3 finalScale = axeFinalScale;
        Vector3 startScale = axeFinalScale * 0.05f;

        axe.transform.localScale = startScale;

        Rigidbody axeRb = axe.GetComponent<Rigidbody>();

        if (axeRb != null)
        {
            axeRb.useGravity = false;
            axeRb.isKinematic = true;
            axeRb.linearVelocity = Vector3.zero;
            axeRb.angularVelocity = Vector3.zero;
        }

        Collider[] colliders = axe.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            col.enabled = true;

        float t = 0f;

        while (t < axeAppearDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / axeAppearDuration);

            axe.transform.localScale =
                Vector3.Lerp(startScale, finalScale, progress);

            yield return null;
        }

        axe.transform.localScale = finalScale;

        if (axeRb != null)
        {
            axeRb.useGravity = true;
            axeRb.isKinematic = false;
        }
    }

    IEnumerator WrongCraftAnimation()
    {
        isCrafting = true;
        HideUI();

        yield return new WaitForSeconds(0.2f);

        foreach (GameObject obj in placedObjects)
        {
            obj.tag = "Pickup";

            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                Vector3 randomDir = new Vector3(
                    Random.Range(-1f, 1f),
                    wrongUpForce,
                    Random.Range(-1f, 1f)
                ).normalized;

                rb.AddForce(randomDir * wrongThrowForce, ForceMode.Impulse);
            }
        }

        placedObjects.Clear();
        isCrafting = false;

        Debug.Log("WRONG RECIPE - OBJECTS EJECTED");
    }
}