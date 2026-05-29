using UnityEngine;
using UnityEngine.InputSystem;

public class PaperReadUI : MonoBehaviour
{
    [HideInInspector] public ReadablePaper ownerPaper;

    [Header("Owner")]
    public int ownerPlayerIndex = 1;

    [Header("Pages")]
    public GameObject[] titleObjects;
    public GameObject[] textObjects;

    [Header("Close Icons")]
    public GameObject keyboardCloseIcon;
    public GameObject controllerCloseIcon;

    private int currentPage = 0;
    private bool usingController = true;
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

        pageInputCooldown -= Time.deltaTime;

        Gamepad pad = GetAssignedGamepad();

        bool nextKeyboard =
            ownerPlayerIndex == 0 &&
            Keyboard.current != null &&
            (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame);

        bool prevKeyboard =
            ownerPlayerIndex == 0 &&
            Keyboard.current != null &&
            (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame);

        bool nextController =
            pad != null &&
            pageInputCooldown <= 0f &&
            (pad.dpad.right.isPressed || pad.leftStick.ReadValue().x > 0.6f);

        bool prevController =
            pad != null &&
            pageInputCooldown <= 0f &&
            (pad.dpad.left.isPressed || pad.leftStick.ReadValue().x < -0.6f);

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

        bool closeKeyboard =
            ownerPlayerIndex == 0 &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame;

        bool closeController =
            pad != null &&
            pad.buttonEast.wasPressedThisFrame; // rond PS5

        if (closeKeyboard || closeController)
            CloseUI();
    }

    Gamepad GetAssignedGamepad()
    {
        if (Gamepad.all.Count <= ownerPlayerIndex)
            return null;

        return Gamepad.all[ownerPlayerIndex];
    }

    void CloseUI()
    {
        if (ownerPaper != null)
            ownerPaper.ClosePaper();

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
        Gamepad pad = GetAssignedGamepad();

        if (pad != null)
        {
            Vector2 dpad = pad.dpad.ReadValue();
            Vector2 stick = pad.leftStick.ReadValue();

            if (dpad != Vector2.zero || stick.magnitude > 0.1f || pad.buttonEast.wasPressedThisFrame)
                usingController = true;
        }

        if (ownerPlayerIndex == 0 && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (ownerPlayerIndex == 0 && Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
    }
}