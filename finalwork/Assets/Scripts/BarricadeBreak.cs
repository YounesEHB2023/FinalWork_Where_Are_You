using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BarricadeBreak : MonoBehaviour
{
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
    public float axeSwingAngle = 45f;

    private int currentHits = 0;
    private bool playerInside = false;
    private bool usingController = false;
    private bool isAnimating = false;
    private bool broken = false;

    void Start()
    {
        HideUI();
    }

    void Update()
    {
        DetectInputDevice();
        UpdateUI();

        bool mouseHit =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool controllerHit =
            Gamepad.current != null &&
            Gamepad.current.buttonWest.wasPressedThisFrame;

        if (playerInside && HasAxeSelected() && !isAnimating && !broken && (mouseHit || controllerHit))
        {
            HitBarricade();
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
                Gamepad.current.buttonWest.wasPressedThisFrame
            )
            {
                usingController = true;
            }
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
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
        if (inventorySystem == null) return null;
        return inventorySystem.GetSelectedItem();
    }

    void HitBarricade()
    {
        currentHits++;

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        StartCoroutine(HitAnimation());

        if (currentHits >= hitsNeeded)
        {
            StartCoroutine(BreakBarricade());
        }
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
        Quaternion hitRot = startRot * Quaternion.Euler(axeSwingAngle, 0f, 0f);

        float halfDuration = axeSwingDuration / 2f;
        float t = 0f;

        while (t < halfDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / halfDuration);

            axeTransform.localRotation = Quaternion.Lerp(startRot, hitRot, progress);

            yield return null;
        }

        t = 0f;

        while (t < halfDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / halfDuration);

            axeTransform.localRotation = Quaternion.Lerp(hitRot, startRot, progress);

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
        bool showUI = playerInside && HasAxeSelected() && !broken && !isAnimating;

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
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            UpdateUI();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            HideUI();
        }
    }
}