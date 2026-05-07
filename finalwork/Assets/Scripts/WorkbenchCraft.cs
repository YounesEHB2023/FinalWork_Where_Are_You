using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorkbenchCraft : MonoBehaviour
{
    [Header("Slots")]
    public Transform[] slots;

    [Header("UI")]
    public GameObject pressEUI;
    public GameObject pressXUI;

    [Header("Animation")]
    public float placeDuration = 0.4f;

    private List<GameObject> placedObjects = new List<GameObject>();

    private bool crafted = false;
    private bool playerInside = false;
    private bool usingController = false;
    private bool isPlacing = false;

    private GameObject currentObject;

    void Start()
    {
        HideUI();
    }

    void Update()
    {
        DetectInputDevice();
        UpdateUI();

        bool keyboardPlace =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        bool controllerPlace =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (playerInside && currentObject != null && !crafted && !isPlacing && (keyboardPlace || controllerPlace))
        {
            if (!placedObjects.Contains(currentObject) && placedObjects.Count < slots.Length)
            {
                StartCoroutine(PlaceObjectAnimated(currentObject));
            }
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
            {
                usingController = true;
            }
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            usingController = false;

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            usingController = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;

        if (other.CompareTag("Pickup"))
            currentObject = other.gameObject;

        UpdateUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            HideUI();
        }

        if (other.gameObject == currentObject)
            currentObject = null;

        UpdateUI();
    }

    IEnumerator PlaceObjectAnimated(GameObject obj)
    {
        isPlacing = true;
        HideUI();

        placedObjects.Add(obj);

        int slotIndex = placedObjects.Count - 1;
        Transform targetSlot = slots[slotIndex];

        obj.tag = "Untagged";

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Vector3 startPos = obj.transform.position;
        Quaternion startRot = obj.transform.rotation;

        float t = 0f;

        while (t < placeDuration)
        {
            t += Time.deltaTime;
            float progress = t / placeDuration;

            obj.transform.position = Vector3.Lerp(startPos, targetSlot.position, progress);
            obj.transform.rotation = Quaternion.Lerp(startRot, targetSlot.rotation, progress);

            yield return null;
        }

        obj.transform.position = targetSlot.position;
        obj.transform.rotation = targetSlot.rotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        currentObject = null;
        isPlacing = false;

        CheckRecipe();
    }

    void UpdateUI()
    {
        bool showUI =
            playerInside &&
            currentObject != null &&
            !crafted &&
            !isPlacing &&
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

    void CheckRecipe()
    {
        bool hasStone = false;
        bool hasStick = false;
        bool hasVine = false;

        foreach (GameObject obj in placedObjects)
        {
            if (obj.name.Contains("Correct_Stone"))
                hasStone = true;

            if (obj.name.Contains("Correct_Stick"))
                hasStick = true;

            if (obj.name.Contains("Correct_Vine"))
                hasVine = true;
        }

        if (hasStone && hasStick && hasVine && placedObjects.Count == 3)
        {
            crafted = true;
            HideUI();
            Debug.Log("CORRECT RECIPE");
        }
        else if (placedObjects.Count == 3)
        {
            HideUI();
            Debug.Log("WRONG RECIPE");
        }
    }
}