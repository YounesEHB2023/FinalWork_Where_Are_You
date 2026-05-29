using UnityEngine;
using UnityEngine.InputSystem;

public class ReadablePaper : MonoBehaviour
{
    [Header("Owner")]
    public int ownerPlayerIndex = 1;

    [Header("UI")]
    public PaperReadUI paperUI;
    public GameObject pressEUI;
    public GameObject pressXUI;

    private bool playerInside;
    private bool usingController = true;
    private bool isReading;

    private MonoBehaviour[] disabledScripts;
    private PickupSystem playerPickupSystem;

    void Start()
    {
        if (paperUI != null)
        {
            paperUI.ownerPaper = this;
            paperUI.ownerPlayerIndex = ownerPlayerIndex;
            paperUI.gameObject.SetActive(false);
        }

        HidePrompt();
    }

    void Update()
    {
        DetectInputDevice();

        if (!isReading)
            UpdatePrompt();

        if (!playerInside || isReading) return;

        if (PressedOpen())
            OpenPaper();
    }

    bool PressedOpen()
    {
        bool keyboardPressed =
            ownerPlayerIndex == 0 &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        Gamepad pad = GetAssignedGamepad();

        bool controllerPressed =
            pad != null &&
            pad.buttonSouth.wasPressedThisFrame;

        return keyboardPressed || controllerPressed;
    }

    Gamepad GetAssignedGamepad()
    {
        if (Gamepad.all.Count <= ownerPlayerIndex)
            return null;

        return Gamepad.all[ownerPlayerIndex];
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup.playerIndex != ownerPlayerIndex) return;

        playerPickupSystem = pickup;
        playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup != playerPickupSystem) return;

        playerInside = false;
        playerPickupSystem = null;

        HidePrompt();
    }

    void OpenPaper()
    {
        if (paperUI == null) return;

        isReading = true;
        HidePrompt();
        DisablePlayerControls();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        paperUI.Open(this);
    }

    public void ClosePaper()
    {
        isReading = false;
        EnablePlayerControls();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        HidePrompt();
    }

    void DisablePlayerControls()
    {
        if (playerPickupSystem == null) return;

        GameObject player = playerPickupSystem.gameObject;
        disabledScripts = player.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in disabledScripts)
        {
            if (script == null) continue;

            string scriptName = script.GetType().Name;

            if (
                scriptName.Contains("FirstPersonController") ||
                scriptName.Contains("PlayerMovement") ||
                scriptName.Contains("PickupSystem") ||
                scriptName.Contains("PlayerInteract")
            )
            {
                script.enabled = false;
            }
        }
    }

    void EnablePlayerControls()
    {
        if (disabledScripts == null) return;

        foreach (MonoBehaviour script in disabledScripts)
        {
            if (script == null) continue;

            string scriptName = script.GetType().Name;

            if (
                scriptName.Contains("FirstPersonController") ||
                scriptName.Contains("PlayerMovement") ||
                scriptName.Contains("PickupSystem") ||
                scriptName.Contains("PlayerInteract")
            )
            {
                script.enabled = true;
            }
        }
    }

    void UpdatePrompt()
    {
        bool showUI = playerInside && !isReading;

        if (!showUI)
        {
            HidePrompt();
            return;
        }

        if (pressEUI != null)
            pressEUI.SetActive(!usingController);

        if (pressXUI != null)
            pressXUI.SetActive(usingController);
    }

    void HidePrompt()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
        if (pressXUI != null) pressXUI.SetActive(false);
    }

    void DetectInputDevice()
    {
        Gamepad pad = GetAssignedGamepad();

        if (pad != null)
        {
            Vector2 dpad = pad.dpad.ReadValue();
            Vector2 stick = pad.leftStick.ReadValue();

            if (dpad != Vector2.zero || stick.magnitude > 0.1f)
                usingController = true;
        }

        if (ownerPlayerIndex == 0 && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (ownerPlayerIndex == 0 && Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
    }
}