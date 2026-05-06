using System.Collections;
using UnityEngine;

public class TransferTunnel : MonoBehaviour
{
    public Transform inPoint;
    public Transform targetSpawnPoint;

    public GameObject pressEUI;
    public GameObject pressXUI;

    [Header("Controls")]
    public KeyCode keyboardTransferKey = KeyCode.E;
    public KeyCode controllerTransferKey = KeyCode.JoystickButton0; // X PS5 souvent ici

    private GameObject currentObject;
    private bool playerInside;
    private bool isTransferring;
    private bool usingController;

    void Start()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
        if (pressXUI != null) pressXUI.SetActive(false);
    }

    void Update()
    {
        DetectLastInput();

        bool keyboardTransfer = Input.GetKeyDown(keyboardTransferKey);
        bool controllerTransfer = Input.GetKeyDown(controllerTransferKey);

        if (playerInside && currentObject != null && !isTransferring && (keyboardTransfer || controllerTransfer))
        {
            StartCoroutine(TransferObject());
        }

        UpdateUI();
    }

    void DetectLastInput()
{
    if (
        Input.GetKeyDown(KeyCode.E) ||
        Input.GetKeyDown(KeyCode.W) ||
        Input.GetKeyDown(KeyCode.A) ||
        Input.GetKeyDown(KeyCode.S) ||
        Input.GetKeyDown(KeyCode.D) ||
        Input.GetKeyDown(KeyCode.Z) ||
        Input.GetKeyDown(KeyCode.Q) ||
        Input.GetMouseButtonDown(0) ||
        Input.GetMouseButtonDown(1) ||
        Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f ||
        Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f
    )
    {
        usingController = false;
    }

    if (
        Input.GetKeyDown(KeyCode.JoystickButton0) ||
        Input.GetKeyDown(KeyCode.JoystickButton1) ||
        Input.GetKeyDown(KeyCode.JoystickButton2) ||
        Input.GetKeyDown(KeyCode.JoystickButton3) ||
            Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f ||
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f
    )
    {
        usingController = true;
    }
}

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            UpdateUI();
        }

        if (other.CompareTag("Pickup"))
        {
            currentObject = other.gameObject;
            UpdateUI();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            UpdateUI();
        }

        if (other.gameObject == currentObject)
        {
            currentObject = null;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        bool showUI = playerInside && currentObject != null && !isTransferring;

        if (pressEUI != null)
            pressEUI.SetActive(showUI && !usingController);

        if (pressXUI != null)
            pressXUI.SetActive(showUI && usingController);
    }

    IEnumerator TransferObject()
    {
        isTransferring = true;
        UpdateUI();

        GameObject objectToTransfer = currentObject;

        Rigidbody rb = objectToTransfer.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Vector3 startPos = objectToTransfer.transform.position;
        Quaternion startRot = objectToTransfer.transform.rotation;

        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            objectToTransfer.transform.position = Vector3.Lerp(startPos, inPoint.position, progress);
            objectToTransfer.transform.rotation = Quaternion.Lerp(startRot, inPoint.rotation, progress);

            yield return null;
        }

        objectToTransfer.transform.position = targetSpawnPoint.position;
        objectToTransfer.transform.rotation = targetSpawnPoint.rotation;

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        currentObject = null;
        isTransferring = false;
        UpdateUI();
    }
}