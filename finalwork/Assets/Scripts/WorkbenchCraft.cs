using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class WorkbenchCraft : NetworkBehaviour
{
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
    public float placeDuration = 0.4f;
    public float fusionDuration = 0.6f;
    public float axeAppearDuration = 0.5f;
    public float wrongThrowForce = 4f;
    public float wrongUpForce = 2f;

    private List<GameObject> placedObjects = new List<GameObject>();

    private bool crafted = false;
    private bool playerInside = false;
    private bool usingController = false;
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

        bool keyboardPlace =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        bool controllerPlace =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (
            playerInside &&
            playerInventory != null &&
            playerInventory.GetSelectedItem() != null &&
            !crafted &&
            !isPlacing &&
            !isCrafting &&
            placedObjects.Count < slots.Length &&
            (keyboardPlace || controllerPlace)
        )
        {
            GameObject selectedItem = playerInventory.GetSelectedItem();
            NetworkObject itemNetObj = selectedItem.GetComponent<NetworkObject>();

            if (itemNetObj == null)
            {
                Debug.LogWarning("Workbench item needs NetworkObject: " + selectedItem.name);
                return;
            }

            Vector3 startPos = selectedItem.transform.position;
Quaternion startRot = selectedItem.transform.rotation;

if (playerPickupSystem != null && playerPickupSystem.GetHeldVisual() != null)
{
    startPos = playerPickupSystem.GetHeldVisual().transform.position;
    startRot = playerPickupSystem.GetHeldVisual().transform.rotation;
}

RequestPlaceObjectServerRpc(itemNetObj.NetworkObjectId, startPos, startRot);

playerInventory.RemoveItem(selectedItem);

if (playerPickupSystem != null)
    playerPickupSystem.ClearHandAfterTransfer();
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj != null && !playerNetObj.IsOwner) return;

        playerInventory = other.GetComponentInChildren<InventorySystem>(true);
        playerPickupSystem = other.GetComponentInChildren<PickupSystem>(true);

        playerInside = true;
        UpdateUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj != null && !playerNetObj.IsOwner) return;

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

        if (pressEUI != null)
            pressEUI.SetActive(showUI && !usingController);

        if (pressXUI != null)
            pressXUI.SetActive(showUI && usingController);
    }

    void HideUI()
    {
        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (pressXUI != null)
            pressXUI.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
void RequestPlaceObjectServerRpc(ulong networkObjectId, Vector3 startPos, Quaternion startRot)
{
    PlaceObjectClientRpc(networkObjectId, startPos, startRot);
}

[ClientRpc]
void PlaceObjectClientRpc(ulong networkObjectId, Vector3 startPos, Quaternion startRot)
{
    if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        return;

    if (placedObjects.Contains(netObj.gameObject)) return;
    if (placedObjects.Count >= slots.Length) return;

    StartCoroutine(PlaceObjectAnimated(netObj.gameObject, startPos, startRot));
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

    float smoothProgress =
        Mathf.SmoothStep(0f, 1f, progress);

    Vector3 arcOffset =
        Vector3.up * Mathf.Sin(smoothProgress * Mathf.PI) * 0.25f;

    obj.transform.position =
        Vector3.Lerp(startPos, targetSlot.position, smoothProgress)
        + arcOffset;

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

        for (int i = 0; i < placedObjects.Count; i++)
{
    placedObjects[i].transform.position = craftCenter.position;
    placedObjects[i].transform.localScale = Vector3.zero;
}

        if (IsServer)
        {
            foreach (GameObject obj in placedObjects)
            {
                NetworkObject netObj = obj.GetComponent<NetworkObject>();

                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn(true);
                else
                    Destroy(obj);
            }

            GameObject axe = Instantiate(axePrefab, axeSpawnPoint.position, axeSpawnPoint.rotation);
            axe.transform.localScale = Vector3.zero;

            NetworkObject axeNetObj = axe.GetComponent<NetworkObject>();

            if (axeNetObj != null)
                axeNetObj.Spawn();

            ShowCraftedAxeClientRpc(axeNetObj.NetworkObjectId);
        }

        placedObjects.Clear();

        crafted = true;
        isCrafting = false;

        Debug.Log("AXE CREATED");
    }

    [ClientRpc]
    void ShowCraftedAxeClientRpc(ulong axeNetworkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(axeNetworkObjectId, out NetworkObject axeNetObj))
            return;

        StartCoroutine(ShowAxeAnimation(axeNetObj.gameObject));
    }

    IEnumerator ShowAxeAnimation(GameObject axe)
    {
        axe.transform.position = axeSpawnPoint.position;
        axe.transform.rotation = axeSpawnPoint.rotation;
        axe.transform.localScale = Vector3.zero;
        axe.tag = "Pickup";

        Rigidbody axeRb = axe.GetComponent<Rigidbody>();

        if (axeRb != null)
        {
            axeRb.useGravity = false;
            axeRb.isKinematic = true;
        }

        float t = 0f;

        while (t < axeAppearDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / axeAppearDuration);

            axe.transform.localScale =
                Vector3.Lerp(Vector3.zero, axeFinalScale, progress);

            yield return null;
        }

        axe.transform.localScale = axeFinalScale;

        if (axeRb != null)
        {
            axeRb.useGravity = true;
            axeRb.isKinematic = false;
        }

       axe.tag = "Pickup";
axe.SetActive(true);

Collider[] colliders = axe.GetComponentsInChildren<Collider>();
foreach (Collider col in colliders)
    col.enabled = true;

Outline outline = axe.GetComponent<Outline>();
if (outline != null)
    outline.enabled = false;

OutlineProximity outlineProximity = axe.GetComponent<OutlineProximity>();
if (outlineProximity != null)
{
    outlineProximity.enabled = false;
    outlineProximity.enabled = true;
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