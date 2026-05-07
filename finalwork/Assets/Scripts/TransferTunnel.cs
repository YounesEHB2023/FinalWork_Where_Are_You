using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransferTunnel : MonoBehaviour
{
    public Transform inPoint;
    public Transform targetSpawnPoint;

    public GameObject pressEUI;
    public GameObject pressXUI;

    [Header("Controls")]
    public KeyCode keyboardTransferKey = KeyCode.E;

    private GameObject currentObject;
    private bool playerInside;
    private bool isTransferring;
    private bool usingController;

    private static TransferTunnel activeTunnel;

    void Start()
    {
        HideUI();
    }

    void Update()
    {
        DetectInputDevice();

        if (activeTunnel == this)
        {
            UpdateUI();

            bool keyboardTransfer =
                Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame;

            bool controllerTransfer =
                Gamepad.current != null &&
                Gamepad.current.buttonSouth.wasPressedThisFrame;

            if (playerInside && currentObject != null && !isTransferring && (keyboardTransfer || controllerTransfer))
            {
                StartCoroutine(TransferObject());
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
                Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame
            )
            {
                usingController = true;
            }
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            usingController = false;
        }

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
        {
            usingController = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            activeTunnel = this;
        }

        if (other.CompareTag("Pickup"))
        {
            currentObject = other.gameObject;
        }

        UpdateUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (activeTunnel == this)
            {
                activeTunnel = null;
                HideUI();
            }
        }

        if (other.gameObject == currentObject)
        {
            currentObject = null;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (activeTunnel != this)
            return;

        bool showUI = playerInside && currentObject != null && !isTransferring;

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

    IEnumerator TransferObject()
    {
        isTransferring = true;
        HideUI();

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

            objectToTransfer.transform.position =
                Vector3.Lerp(startPos, inPoint.position, progress);

            objectToTransfer.transform.rotation =
                Quaternion.Lerp(startRot, inPoint.rotation, progress);

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