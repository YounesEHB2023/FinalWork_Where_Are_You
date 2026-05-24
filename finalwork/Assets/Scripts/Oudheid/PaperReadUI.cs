using UnityEngine;
using UnityEngine.InputSystem;

public class PaperReadUI : MonoBehaviour
{
    [HideInInspector] public ReadablePaper ownerPaper;

    [Header("Pages")]
    public GameObject[] titleObjects;
    public GameObject[] textObjects;

    [Header("Close Icons")]
    public GameObject keyboardCloseIcon;
    public GameObject controllerCloseIcon;

    private int currentPage = 0;
    private bool usingController;
    private float pageInputCooldown;

    public void Open(ReadablePaper readablePaper)
    {
        ownerPaper = readablePaper;
        currentPage = 0;
        gameObject.SetActive(true);
        UpdatePage();
    }

    void Update()
    {
        DetectInputDevice();
        UpdateCloseIcon();

        pageInputCooldown -= Time.unscaledDeltaTime;

        bool nextKeyboard = Keyboard.current != null &&
            (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame);

        bool prevKeyboard = Keyboard.current != null &&
            (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame);

        bool nextController = Gamepad.current != null &&
            pageInputCooldown <= 0f &&
            (Gamepad.current.dpad.right.isPressed || Gamepad.current.leftStick.ReadValue().x > 0.6f);

        bool prevController = Gamepad.current != null &&
            pageInputCooldown <= 0f &&
            (Gamepad.current.dpad.left.isPressed || Gamepad.current.leftStick.ReadValue().x < -0.6f);

        if (nextKeyboard || nextController)
        {
            currentPage++;
            if (currentPage >= titleObjects.Length)
                currentPage = 0;

            pageInputCooldown = 0.25f;
            UpdatePage();
        }

        if (prevKeyboard || prevController)
        {
            currentPage--;
            if (currentPage < 0)
                currentPage = titleObjects.Length - 1;

            pageInputCooldown = 0.25f;
            UpdatePage();
        }

        bool closeKeyboard = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool closeController = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

        if (closeKeyboard || closeController)
            CloseUI();
    }

    void CloseUI()
    {
        Debug.Log("PAPER UI CLOSE BUTTON");

        if (ownerPaper != null)
            ownerPaper.ClosePaper();
        else
            Debug.LogWarning("ownerPaper is NULL");

        gameObject.SetActive(false);
    }

    void UpdatePage()
    {
        for (int i = 0; i < titleObjects.Length; i++)
        {
            if (titleObjects[i] != null)
                titleObjects[i].SetActive(i == currentPage);

            if (textObjects[i] != null)
                textObjects[i].SetActive(i == currentPage);
        }
    }

    void UpdateCloseIcon()
    {
        if (keyboardCloseIcon != null)
            keyboardCloseIcon.SetActive(!usingController);

        if (controllerCloseIcon != null)
            controllerCloseIcon.SetActive(usingController);
    }

    void DetectInputDevice()
    {
        if (Gamepad.current != null)
        {
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            if (dpad != Vector2.zero || stick.magnitude > 0.1f || Gamepad.current.buttonEast.wasPressedThisFrame)
                usingController = true;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
    }
}