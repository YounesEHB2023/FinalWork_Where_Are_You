using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorkbenchCraft : MonoBehaviour
{
    [Header("Slots")]
    public Transform[] slots;
    public Transform craftCenter;
    public Transform axeSpawnPoint;

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

    private GameObject currentObject;

    void Start()
    {
        HideUI();
    }

    void Update()
    {
        DetectInputDevice();
        UpdateUI();

        bool keyboardPlace = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool controllerPlace = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (playerInside && currentObject != null && !crafted && !isPlacing && !isCrafting && (keyboardPlace || controllerPlace))
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

            if (dpad != Vector2.zero || leftStick.magnitude > 0.1f || rightStick.magnitude > 0.1f || Gamepad.current.buttonSouth.wasPressedThisFrame)
                usingController = true;
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

        currentObject = null;
        isPlacing = false;

        CheckRecipe();
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
        {
            StartCoroutine(CorrectCraftAnimation());
        }
        else
        {
            StartCoroutine(WrongCraftAnimation());
        }
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
                placedObjects[i].transform.position = Vector3.Lerp(startPositions[i], craftCenter.position, progress) + shake;
                placedObjects[i].transform.localScale = Vector3.Lerp(startScales[i], Vector3.zero, progress);
            }

            yield return null;
        }

        foreach (GameObject obj in placedObjects)
        {
            Destroy(obj);
        }

        placedObjects.Clear();

        GameObject axe = Instantiate(axePrefab, axeSpawnPoint.position, axeSpawnPoint.rotation);
        axe.transform.localScale = Vector3.zero;

        t = 0f;

        while (t < axeAppearDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / axeAppearDuration);

            axe.transform.localScale = Vector3.Lerp(Vector3.zero, axeFinalScale, progress);

            yield return null;
        }

        axe.transform.localScale = axeFinalScale;
        axe.tag = "Pickup";

        Rigidbody axeRb = axe.GetComponent<Rigidbody>();
        if (axeRb != null)
        {
            axeRb.useGravity = true;
            axeRb.isKinematic = false;
        }

        crafted = true;
        isCrafting = false;

        Debug.Log("AXE CREATED");
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

    void UpdateUI()
    {
        bool showUI =
            playerInside &&
            currentObject != null &&
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
}