using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public int playerIndex = 0;

    [Header("UI")]
    public Image[] slotBackgrounds;
    public Image[] itemIcons;

    [Header("Input UI")]
    public GameObject[] keyboardKeys;
    public GameObject[] controllerArrows;

    public Color selectedColor = Color.white;
    public Color normalColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("Settings")]
    public int maxSlots = 3;

    private GameObject[] items;
    private int currentIndex = 0;

    void Awake()
    {
        FirstPersonController controller = GetComponentInParent<FirstPersonController>();
        if (controller != null)
            playerIndex = controller.playerIndex;
    }

    void Start()
    {
        items = new GameObject[maxSlots];
        SetInputUI(true);
        UpdateUI();
    }

    void Update()
    {
        if (!Application.isFocused) return;

        // Keyboard only for Player 1 testing
        if (playerIndex == 0 && Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
        }
    }

    Gamepad GetAssignedGamepad()
    {
        if (Gamepad.all.Count <= playerIndex)
            return null;

        return Gamepad.all[playerIndex];
    }

    void SetInputUI(bool controller)
    {
        foreach (GameObject key in keyboardKeys)
        {
            if (key != null)
                key.SetActive(!controller);
        }

        foreach (GameObject arrow in controllerArrows)
        {
            if (arrow != null)
                arrow.SetActive(controller);
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

    public GameObject GetSelectedItem()
    {
        return items[currentIndex];
    }

    public bool AddItemToFirstEmptySlot(GameObject item, Sprite icon)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;

                if (itemIcons[i] != null)
                {
                    itemIcons[i].sprite = icon;
                    itemIcons[i].enabled = true;
                }

                SelectSlot(i);
                UpdateUI();
                return true;
            }
        }

        return false;
    }

    public void RemoveItem(GameObject item)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;

                if (itemIcons[i] != null)
                {
                    itemIcons[i].sprite = null;
                    itemIcons[i].enabled = false;
                }

                UpdateUI();
                return;
            }
        }
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

    public bool HasItemByName(string itemName)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (items[i] != null && items[i].name.ToLower().Contains(itemName.ToLower()))
                return true;
        }

        return false;
    }

    public GameObject GetItemByName(string itemName)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (items[i] != null && items[i].name.ToLower().Contains(itemName.ToLower()))
                return items[i];
        }

        return null;
    }
}