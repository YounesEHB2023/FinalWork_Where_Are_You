using UnityEngine;
using UnityEngine.InputSystem;

public class PickupSystem : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform holdPoint;

    [Header("Pickup Settings")]
    public float pickupDistance = 3f;
    public float dropForwardForce = 1f;

    private GameObject heldObject;
    private Rigidbody heldRb;

    void Update()
    {
        if (PressedInteract())
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else
            {
                DropObject();
            }
        }
    }

    bool PressedInteract()
    {
        bool keyboardPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        bool controllerPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        // buttonSouth = X op PS5

        return keyboardPressed || controllerPressed;
    }

    void TryPickup()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                heldObject = hit.collider.gameObject;
                heldRb = heldObject.GetComponent<Rigidbody>();

                if (heldRb != null)
                {
                    heldRb.linearVelocity = Vector3.zero;
heldRb.angularVelocity = Vector3.zero;
heldRb.useGravity = false;
heldRb.isKinematic = true;
                }

                heldObject.transform.SetParent(holdPoint);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void DropObject()
    {
        heldObject.transform.SetParent(null);

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = true;

            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;

            heldRb.AddForce(playerCamera.transform.forward * dropForwardForce, ForceMode.Impulse);
        }

        heldObject = null;
        heldRb = null;
    }
}