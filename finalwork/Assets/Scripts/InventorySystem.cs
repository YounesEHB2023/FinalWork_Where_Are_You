using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("UI")]
    public Image[] slotBackgrounds;
    public Image[] itemIcons;

    [Header("Input UI")]
    public GameObject[] keyboardKeys; // Key1, Key2, Key3
    public GameObject[] controllerArrows; // ArrowLeft, ArrowRight

    public Color selectedColor = Color.white;
    public Color normalColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("Settings")]
    public int maxSlots = 3;

    private GameObject[] items;
    private int currentIndex = 0;
    private bool usingController = false;

    void Start()
    {
        items = new GameObject[maxSlots];
SetInputUI(true);        UpdateUI();
    }

    void Update()
    {
        HandleSelection();
        DetectInputDevice();
    }

    void HandleSelection()
    {
        // Keyboard selection
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                SelectSlot(0);

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                SelectSlot(1);

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
                SelectSlot(2);
        }

        // Controller navigation
        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.right.wasPressedThisFrame)
                SelectNextSlot();

            if (Gamepad.current.dpad.left.wasPressedThisFrame)
                SelectPreviousSlot();
        }
    }

    void DetectInputDevice()
    {
        if (Gamepad.current != null)
        {
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();

            if (dpad != Vector2.zero || leftStick.magnitude > 0.1f || rightStick.magnitude > 0.1f)
                SetInputUI(true);
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
                SetInputUI(false);
        }

    if (Mouse.current != null)
{
    if (Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
    {
        SetInputUI(false);
    }
}
    }

    void SetInputUI(bool controller)
    {
        if (usingController == controller) return;

        usingController = controller;

        foreach (GameObject key in keyboardKeys)
        {
            if (key != null)
                key.SetActive(!usingController);
        }

        foreach (GameObject arrow in controllerArrows)
        {
            if (arrow != null)
                arrow.SetActive(usingController);
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= maxSlots) return;

        currentIndex = index;
        UpdateUI();
    }

    public void SelectNextSlot()
    {
        currentIndex++;

        if (currentIndex >= maxSlots)
            currentIndex = 0;

        UpdateUI();
    }

    public void SelectPreviousSlot()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = maxSlots - 1;

        UpdateUI();
    }

    public bool AddItemToSelectedSlot(GameObject item, Sprite icon)
    {
        return AddItemToSlot(currentIndex, item, icon);
    }

    public bool AddItemToSlot(int slotIndex, GameObject item, Sprite icon)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return false;
        if (items[slotIndex] != null) return false;

        items[slotIndex] = item;

        if (itemIcons[slotIndex] != null)
        {
            itemIcons[slotIndex].sprite = icon;
            itemIcons[slotIndex].enabled = true;
        }

        SelectSlot(slotIndex);
        UpdateUI();

        return true;
    }

    public GameObject GetSelectedItem()
    {
        return items[currentIndex];
    }

    public void RemoveSelectedItem()
    {
        items[currentIndex] = null;

        if (itemIcons[currentIndex] != null)
        {
            itemIcons[currentIndex].sprite = null;
            itemIcons[currentIndex].enabled = false;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (slotBackgrounds[i] != null)
                slotBackgrounds[i].color = i == currentIndex ? selectedColor : normalColor;

            if (itemIcons[i] != null)
                itemIcons[i].enabled = items[i] != null;
        }
    }
}