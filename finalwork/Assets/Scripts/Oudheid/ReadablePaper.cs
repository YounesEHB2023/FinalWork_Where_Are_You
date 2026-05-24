using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class ReadablePaper : MonoBehaviour
{
    public PaperReadUI paperUI;
    public GameObject pressEUI;
    public GameObject pressXUI;

    private bool playerInside;
    private bool usingController;
    private bool isReading;

    private MonoBehaviour[] disabledScripts;

    void Start()
    {
        if (paperUI != null)
        {
            paperUI.ownerPaper = this;
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

        bool pressed =
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (pressed)
            OpenPaper();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsOwner) return;

        playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsOwner) return;

        playerInside = false;
        HidePrompt();
    }

    void OpenPaper()
    {
        if (paperUI == null) return;

        Debug.Log("OPEN PAPER");

        isReading = true;
        HidePrompt();
        DisablePlayerControls();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        paperUI.Open(this);
    }

    public void ClosePaper()
    {
        Debug.Log("CLOSE PAPER");

        isReading = false;
        EnablePlayerControls();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        HidePrompt();
    }

    void DisablePlayerControls()
    {
        GameObject player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
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
                Debug.Log("DISABLE: " + scriptName);
                script.enabled = false;
            }
        }
    }

    void EnablePlayerControls()
    {
        if (disabledScripts == null)
        {
            Debug.LogWarning("No disabled scripts found.");
            return;
        }

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
                Debug.Log("ENABLE: " + scriptName);
                script.enabled = true;
            }
        }
    }

    void UpdatePrompt()
    {
        if (!playerInside)
        {
            HidePrompt();
            return;
        }

        if (pressEUI != null) pressEUI.SetActive(!usingController);
        if (pressXUI != null) pressXUI.SetActive(usingController);
    }

    void HidePrompt()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
        if (pressXUI != null) pressXUI.SetActive(false);
    }

    void DetectInputDevice()
    {
        if (Gamepad.current != null)
        {
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            if (dpad != Vector2.zero || stick.magnitude > 0.1f)
                usingController = true;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
    }
}