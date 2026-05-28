using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BarricadeBreak : MonoBehaviour
{
    [Header("Owner")]
    public int ownerPlayerIndex = 0; // Player 1 = 0

    [Header("References")]
    public InventorySystem inventorySystem;
    public GameObject barricadeVisual;

    [Header("UI")]
    public GameObject pressEUI;
    public GameObject pressXUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;

    [Header("Settings")]
    public int hitsNeeded = 3;
    public string requiredItemName = "Axe";

    [Header("Barricade Animation")]
    public float shakeDuration = 0.25f;
    public float shakeStrength = 0.08f;

    [Header("Axe Animation")]
    public float axeSwingDuration = 0.25f;
    public Vector3 axeSwingRotation = new Vector3(55f, -20f, 25f);

    private int currentHits = 0;
    private bool playerInside = false;
    private bool usingController = true;
    private bool isAnimating = false;
    private bool broken = false;

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
        if (!HasAxeSelected()) return;
        if (isAnimating || broken) return;

        if (PressedHit())
            HitBarricade();
    }

    bool PressedHit()
    {
        bool mouseHit =
            ownerPlayerIndex == 0 &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        Gamepad pad = GetAssignedGamepad(ownerPlayerIndex);

        bool controllerHit =
            pad != null &&
            pad.buttonWest.wasPressedThisFrame; // Carré PS5

        return mouseHit || controllerHit;
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

            if (
                dpad != Vector2.zero ||
                leftStick.magnitude > 0.1f ||
                rightStick.magnitude > 0.1f ||
                pad.buttonWest.wasPressedThisFrame
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

    bool HasAxeSelected()
    {
        if (inventorySystem == null) return false;

        GameObject selectedItem = inventorySystem.GetSelectedItem();
        if (selectedItem == null) return false;

        return selectedItem.name.ToLower().Contains(requiredItemName.ToLower());
    }

    GameObject GetSelectedAxe()
    {
        if (playerPickupSystem == null) return null;

        return playerPickupSystem.GetHeldVisual();
    }

    void HitBarricade()
    {
        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        currentHits++;

        StartCoroutine(HitAnimation());

        if (currentHits >= hitsNeeded)
            StartCoroutine(BreakBarricade());
    }

    IEnumerator HitAnimation()
    {
        isAnimating = true;

        Coroutine shakeRoutine = StartCoroutine(ShakeBarricade());
        Coroutine axeRoutine = StartCoroutine(SwingAxe());

        yield return shakeRoutine;
        yield return axeRoutine;

        isAnimating = false;
    }

    IEnumerator SwingAxe()
    {
        GameObject axe = GetSelectedAxe();

        if (axe == null)
            yield break;

        Transform axeTransform = axe.transform;

        Quaternion startRot = axeTransform.localRotation;
        Quaternion hitRot = startRot * Quaternion.Euler(axeSwingRotation);

        float halfDuration = axeSwingDuration / 2f;
        float t = 0f;

        while (t < halfDuration)
        {
            t += Time.deltaTime;

            float progress = Mathf.SmoothStep(0f, 1f, t / halfDuration);

            axeTransform.localRotation =
                Quaternion.Lerp(startRot, hitRot, progress);

            yield return null;
        }

        t = 0f;

        while (t < halfDuration)
        {
            t += Time.deltaTime;

            float progress = Mathf.SmoothStep(0f, 1f, t / halfDuration);

            axeTransform.localRotation =
                Quaternion.Lerp(hitRot, startRot, progress);

            yield return null;
        }

        axeTransform.localRotation = startRot;
    }

    IEnumerator ShakeBarricade()
    {
        Transform target = barricadeVisual != null ? barricadeVisual.transform : transform;

        Vector3 startPos = target.localPosition;

        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            Vector3 shake = Random.insideUnitSphere * shakeStrength;
            shake.y = 0f;

            target.localPosition = startPos + shake;

            yield return null;
        }

        target.localPosition = startPos;
    }

    IEnumerator BreakBarricade()
    {
        broken = true;
        HideUI();

        if (hitSound != null)
            yield return new WaitForSeconds(hitSound.length);

        if (barricadeVisual != null)
            barricadeVisual.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    void UpdateUI()
    {
        bool showUI =
            playerInside &&
            HasAxeSelected() &&
            !broken &&
            !isAnimating;

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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        InventorySystem inventory = other.GetComponentInChildren<InventorySystem>(true);

        if (pickup == null || inventory == null) return;
        if (pickup.playerIndex != ownerPlayerIndex) return;

        playerPickupSystem = pickup;
        inventorySystem = inventory;
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
        playerPickupSystem = null;
        inventorySystem = null;

        HideUI();
    }
}